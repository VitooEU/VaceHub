[CmdletBinding()]
param([ValidateRange(1, 65535)][int]$BuildNumber = 1)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path $PSScriptRoot -Parent
$project = Join-Path $root 'VaceHubLauncher/VaceHubLauncher.csproj'
$out = Join-Path $root 'artifacts/Xbox'
$cert = $null
$pfxPath = Join-Path $env:RUNNER_TEMP ('VaceHub-' + [guid]::NewGuid() + '.pfx')
New-Item -ItemType Directory -Path $out -Force | Out-Null
try {
    if ($env:VACEHUB_PFX_BASE64) {
        [IO.File]::WriteAllBytes($pfxPath, [Convert]::FromBase64String($env:VACEHUB_PFX_BASE64))
        $password = ConvertTo-SecureString -String $env:VACEHUB_PFX_PASSWORD -AsPlainText -Force
        $cert = Import-PfxCertificate -FilePath $pfxPath -CertStoreLocation Cert:\CurrentUser\My -Password $password
        if (@($cert).Count -ne 1) { throw 'PFX muss genau ein Signaturzertifikat enthalten.' }
    } else {
        $cert = New-SelfSignedCertificate -Type Custom -Subject 'CN=VaceHub' -FriendlyName 'VaceHub Dev Mode Test' `
            -KeyUsage DigitalSignature -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 `
            -KeyExportPolicy NonExportable -CertStoreLocation Cert:\CurrentUser\My `
            -NotAfter (Get-Date).AddYears(1) `
            -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}')
        Write-Warning 'Temporäres Testzertifikat: Bei einem späteren Build kann eine Neuinstallation erforderlich sein. Bibliothek vorher sichern.'
    }
    if ($cert.Subject -ne 'CN=VaceHub' -or !$cert.HasPrivateKey -or $cert.NotAfter -lt (Get-Date)) {
        throw 'Ein gültiges Zertifikat mit privatem Schlüssel und Subject CN=VaceHub ist erforderlich.'
    }
    Export-Certificate -Cert $cert -FilePath (Join-Path $out 'VaceHub-Testzertifikat.cer') | Out-Null
    $packageDir = $out + [IO.Path]::DirectorySeparatorChar
    $arguments = @($project, '/restore', '/m', '/p:Configuration=Release', '/p:Platform=x64',
        '/p:GenerateAppxPackageOnBuild=true', '/p:UapAppxPackageBuildMode=SideloadOnly',
        '/p:AppxBundle=Never', '/p:AppxPackageSigningEnabled=true',
        "/p:PackageCertificateThumbprint=$($cert.Thumbprint)", '/p:PackageCertificateKeyFile=',
        "/p:AppxPackageDir=$packageDir", "/p:AppxManifestIdentityVersion=1.0.$BuildNumber.0",
        '/fl', "/flp:logfile=$root/artifacts/build.log;verbosity=normal")
    & msbuild @arguments
    if ($LASTEXITCODE -ne 0) { throw "MSBuild fehlgeschlagen: $LASTEXITCODE" }
    $packages = @(Get-ChildItem $out -Recurse -File | Where-Object { $_.Extension -in '.appx','.msix' -and $_.FullName -notmatch '[\\/]Dependencies[\\/]' })
    if ($packages.Count -eq 0) { throw 'Build hat kein installierbares App-Paket erzeugt.' }
    Copy-Item (Join-Path $root 'INSTALL-XBOX.md') $out
    # Exclude any accidentally generated private-key file from the downloadable output.
    if (Get-ChildItem $out -Recurse -File -Filter '*.pfx') { throw 'Privater Schlüssel im Ausgabeordner: Upload abgebrochen.' }
    $hashes = Get-ChildItem $out -Recurse -File | ForEach-Object {
        $hash = Get-FileHash $_.FullName -Algorithm SHA256
        '{0}  {1}' -f $hash.Hash.ToLowerInvariant(), [IO.Path]::GetRelativePath($out, $_.FullName).Replace('\','/')
    }
    $hashes | Set-Content (Join-Path $out 'SHA256SUMS.txt') -Encoding utf8
    "## Xbox-Paket erstellt`n`nDownload: **VaceHub-Xbox-x64** unter Artifacts. ZIP auf Ubuntu entpacken; INSTALL-XBOX.md befolgen. Xbox-Hardwaretest steht aus." | Out-File $env:GITHUB_STEP_SUMMARY -Append
} finally {
    if (Test-Path $pfxPath) { Remove-Item $pfxPath -Force }
    if ($cert -and $cert.Thumbprint) { Remove-Item "Cert:\CurrentUser\My\$($cert.Thumbprint)" -ErrorAction SilentlyContinue }
}
