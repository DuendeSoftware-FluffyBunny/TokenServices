$Time = [System.Diagnostics.Stopwatch]::StartNew()

function PrintElapsedTime {
    Log $([string]::Format("Elapsed time: {0}.{1}", $Time.Elapsed.Seconds, $Time.Elapsed.Milliseconds))
}

function Log {
    Param ([string] $s)
    Write-Output "###### $s"
}

function Check {
    Param ([string] $s)
    if ($LASTEXITCODE -ne 0) { 
        Log "Failed: $s"
        throw "Error case -- see failed step"
    }
}

$DockerOS = docker version -f "{{ .Server.Os }}"
$ImageName = "fluffybunny4/build"
$Dockerfile = ".\Dockerfile-Build"

$Version = "0.0.3"

PrintElapsedTime

Log "Build application image"
docker build --no-cache --pull -t $ImageName -f $Dockerfile --build-arg Version=$Version .
PrintElapsedTime
Check "docker build (application)"

# docker build -f ./IdentityServer4WithGrace/Dockerfile -t fluffybunny4/app .
# docker build -f ./IdentityServer4WithGrace/Dockerfile -t fluffybunny4/app .
docker build -f ./Dockerfile-SampleExternalService -t fluffybunny4/sampleexternalservice .
docker tag  fluffybunny4/sampleexternalservice:latest ghstahl/sampleexternalservice:latest
# push to docker hub.
# docker push ghstahl/sampleexternalservice:latest

