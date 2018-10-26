# Copyright (c) 2018 The nanoFramework project contributors
# See LICENSE file in the project root for full license information.

# skip updating dependencies if build is a pull-request or not a tag (master OR release)
if ($env:appveyor_pull_request_number -or 
    ($env:APPVEYOR_REPO_BRANCH -eq "master" -and $env:APPVEYOR_REPO_TAG -eq 'false') -or
    ($env:APPVEYOR_REPO_BRANCH -match "^release*" -and $env:APPVEYOR_REPO_TAG -eq 'false') -or
    $env:APPVEYOR_REPO_TAG -eq "false")
{
    'Skip updating dependencies...' | Write-Host -ForegroundColor White
}
else
{
    # update dependencies for class libraries that depend Runtime.Events and mscorlib

    # because it can take sometime for the package to become available on the NuGet providers
    # need to hange here for 5 minutes (5 * 60 * 1000)
    "Waiting 5 minutes to let package process flow in NuGet providers..." | Write-Host -ForegroundColor White
    Start-Sleep -Milliseconds 300000

    $librariesToUpdate =    ("lib-nanoFramework.System.Net.Http")

    ForEach($library in $librariesToUpdate)
    {
        # init/reset these
        $commitMessage = ""
        $prTitle = ""
        $projectPath = ""
        $newBranchName = "$env:APPVEYOR_REPO_BRANCH-nfbot/update-dependencies"
    
        "Updating $library" | Write-Host -ForegroundColor White
   
        # make sure we are in the projects directory
        &  cd "C:\projects" > $null

        # clone library repo and checkout develop branch
        "Cloning $library" | Write-Host -ForegroundColor White
        git clone "https://github.com/nanoframework/$library" -b develop --depth 1 -q
        cd $library
        cd source
    
        # find solution file in repository
        $solutionFile = (Get-ChildItem -Path ".\" -Include "*.sln" -Recurse)

        # find packages.config
        $packagesConfig = (Get-ChildItem -Path ".\" -Include "packages.config" -Recurse)

        # load packages.config as XML doc
        [xml]$packagesDoc = Get-Content $packagesConfig

        $nodes = $packagesDoc.SelectNodes("*").SelectNodes("*")

        $packageList = @(,@())

        foreach ($node in $nodes)
        {
            # filter out NuProj packages
            if($node.id -notlike "NuProj*")
            {
                if($packageList)
                {
                    $packageList += , ($node.id,  $node.version)
                }
                else
                {
                    $packageList = , ($node.id,  $node.version)
                }
            }
        }

        if ($packageList.length -gt 0)
        {
            "NuGet packages to update:" | Write-Host -ForegroundColor White
            $packageList | Write-Host -ForegroundColor White

            # restore NuGet packages, need to do this before anything else
            nuget restore $solutionFile[0] -Source https://www.myget.org/F/nanoframework-dev/api/v3/index.json -Source https://api.nuget.org/v3/index.json                

            # rename nfproj files to csproj
            Get-ChildItem -Path ".\" -Include "*.nfproj" -Recurse |
                Foreach-object {
                    $OldName = $_.name; 
                    $NewName = $_.name -replace 'nfproj','csproj'; 
                    Rename-Item  -Path $_.fullname -Newname $NewName; 
                }

            # update all packages
            foreach ($package in $packageList)
            {
                # get package name and target version
                $packageName = $package[0]
                $packageOriginVersion = $package[1]
    
                # update package
                if ($env:APPVEYOR_REPO_BRANCH -like '*release*' -or $env:APPVEYOR_REPO_BRANCH -like '*master*')
                {
                    # don't allow prerelease for release and master branches
                    $updatePackage = nuget update $solutionFile[0].FullName -Source https://api.nuget.org/v3/index.json -Source https://api.nuget.org/v3/index.json 
                }
                else
                {
                    # allow prerelease for all others
                    $updatePackage = nuget update $solutionFile[0].FullName -Source https://www.myget.org/F/nanoframework-dev/api/v3/index.json -Source https://api.nuget.org/v3/index.json -PreRelease
                }

                # need to get target version
                # load packages.config as XML doc
                [xml]$packagesDoc = Get-Content $packagesConfig

                $nodes = $packagesDoc.SelectNodes("*").SelectNodes("*")

                foreach ($node in $nodes)
                {
                    # find this package
                    if($node.id -match $packageName)
                    {
                        $packageTargetVersion = $node.version
                    }
                }

                #  grab csproj from update output, if not already there
                if($projectPath -eq "")
                {
                    $projectPath = [regex]::Match($updatePackage, "((project ')(.*)(', targeting))").captures.Groups[3].Value
                }

                # replace NFMDP_PE_LoadHints
                $filecontent = Get-Content($projectPath)
                attrib $projectPath -r
                $filecontent -replace "($packageName.$packageOriginVersion)", "$packageName.$packageTargetVersion" | Out-File $projectPath -Encoding utf8

                # update nuproj files, if any
                $nuprojFiles = (Get-ChildItem -Path ".\" -Include "*.nuproj" -Recurse)

                foreach ($nuproj in $nuprojFiles)
                {
                    [xml]$nuprojDoc = Get-Content $nuproj

                    #$nuprojDoc.Project.ItemGroup

                    $nodes = $nuprojDoc.SelectNodes("*").SelectNodes("*")

                    foreach ($node in $nodes)
                    {
                        if($node.Name -eq "ItemGroup")
                        {
                            foreach ($itemGroup in $node.ChildNodes)
                            {
                                if($itemGroup.Name -eq "Dependency" -and $itemGroup.Attributes["Include"].value -eq $packageName)
                                {
                                    $itemGroup.ChildNodes[0].innertext = "[$packageTargetVersion]"
                                }
                            }
                        }
                    }

                    $nuprojDoc.Save($nuproj[0].FullName)
                }

                #  update branch name
                $tempPackageName = $packageName -replace "(nanoFramework.)", ""
                $newBranchName += "/$tempPackageName.$packageTargetVersion"
                
                # build commit message
                $commitMessage += "Bumps $packageName from $packageOriginVersion to $packageTargetVersion.`n"

                # build PR title
                $prTitle = "Bumps $packageName from $packageOriginVersion to $packageTargetVersion"
            }

            # rename csproj files back to nfproj
            Get-ChildItem -Path ".\" -Include "*.csproj" -Recurse |
            Foreach-object {
                $OldName = $_.name; 
                $NewName = $_.name -replace 'csproj','nfproj'; 
                Rename-Item  -Path $_.fullname -Newname $NewName; 
                }
            
            # need this line so nfbot flags the PR appropriately
            $commitMessage += "`n[version update]`n`n"

            # better add this warning line               
            $commitMessage += "### :warning: This is an automated update. Merge only after all tests pass. :warning:`n"

            # create branch to perform updates
            git branch $newBranchName -q
            
            # checkout branch
            git checkout $newBranchName -q

            # commit changes
            git add -A 2>&1

            # commit message with a different title if one or more dependencies are updated
            if ($packageCount -gt 1)
            {
                git commit -m "Update $packageCount NuGet dependencies" -m"$commitMessage" -q

                # fix PR title
                $prTitle = "Update $packageCount NuGet dependencies"
            }
            else 
            {
                git commit -m "$prTitle" -m "$commitMessage" -q
            }

            git push --set-upstream origin $newBranchName --porcelain -q

            # start PR
            # we are hardcoding to develop branch to have a fixed one
            # this is very important for tags (which don't have branch information)
            # considering that the base branch can be changed at the PR ther is no big deal about this 
            $prRequestBody = @{title="$prTitle";body="$commitMessage";head="$newBranchName";base="develop"} | ConvertTo-Json
            $githubApiEndpoint = "https://api.github.com/repos/nanoframework/$library/pulls"
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

            $headers = @{}
            $headers.Add("Authorization","Basic $env:GitRestAuth")
            $headers.Add("Accept","application/vnd.github.symmetra-preview+json")

            try 
            {
                $result = Invoke-RestMethod -Method Post -UserAgent [Microsoft.PowerShell.Commands.PSUserAgent]::InternetExplorer -Uri  $githubApiEndpoint -Header $headers -ContentType "application/json" -Body $prRequestBody
                'Started PR with dependencies update...' | Write-Host -ForegroundColor White -NoNewline
                'OK' | Write-Host -ForegroundColor Green
            }
            catch 
            {
                $result = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($result)
                $reader.BaseStream.Position = 0
                $reader.DiscardBufferedData()
                $responseBody = $reader.ReadToEnd();

                "Error starting PR: $responseBody" | Write-Host -ForegroundColor Red
            }
        }
        else
        {
            # nothing to update???
            "Couldn't find anything to update..." | Write-Host -ForegroundColor Black -BackgroundColor Yellow
        }
    }

    # get back to the original build folder
    cd $env:APPVEYOR_BUILD_FOLDER
}
