# NexusStack Template Installation Script
# Usage: .\Install-Template.ps1

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "NexusStack Solution Template Installer" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# 自动定位项目根目录
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = $scriptPath

# 检查当前目录是否是 Docs 目录
if ((Split-Path -Leaf $scriptPath) -eq "Docs") {
    # 如果在 Docs 目录，切换到上一级（项目根目录）
    $rootPath = Split-Path -Parent $scriptPath
    Write-Host "检测到脚本在 Docs 目录，切换到项目根目录..." -ForegroundColor Yellow
    Set-Location $rootPath
}

# 验证是否在正确的目录
if (!(Test-Path ".\NexusStack.Template.csproj")) {
    Write-Host "错误: 找不到 NexusStack.Template.csproj 文件！" -ForegroundColor Red
    Write-Host "当前目录: $(Get-Location)" -ForegroundColor Gray
    Write-Host "请确保脚本在项目根目录或 Docs 目录下运行。" -ForegroundColor Yellow
    Read-Host "`n按任意键退出"
    exit 1
}

Write-Host "工作目录: $(Get-Location)" -ForegroundColor Green
Write-Host ""

# 选择安装方式
Write-Host "请选择操作:" -ForegroundColor Yellow
Write-Host "1. 本地安装（开发测试用）"
Write-Host "2. 打包为 NuGet 包"
Write-Host "3. 打包并发布到 NuGet.org"
Write-Host "4. 卸载现有模板"
Write-Host "5. 完全清理并重新安装（推荐）"
Write-Host ""

$choice = Read-Host "请输入选项 (1-5)"

switch ($choice) {
    "1" {
        Write-Host "`n正在本地安装模板..." -ForegroundColor Green

        # 获取当前路径
        $currentPath = (Get-Location).Path

        # 先卸载已存在的模板
        Write-Host "检查是否已安装旧版本..." -ForegroundColor Yellow
        Write-Host "  卸载路径: $currentPath" -ForegroundColor Gray
        dotnet new uninstall $currentPath 2>$null
        dotnet new uninstall . 2>$null

        # 安装新模板
        Write-Host "安装模板..." -ForegroundColor Yellow
        dotnet new install .

        if ($LASTEXITCODE -eq 0) {
            Write-Host "`n✓ 模板安装成功！" -ForegroundColor Green
            Write-Host "`n使用示例:" -ForegroundColor Cyan
            Write-Host "  dotnet new nexusstack -n MyProject" -ForegroundColor White
            Write-Host "`n查看详细参数:" -ForegroundColor Cyan
            Write-Host "  dotnet new nexusstack --help" -ForegroundColor White

            # 显示已安装的模板
            Write-Host "`n已安装的模板:" -ForegroundColor Cyan
            dotnet new list | Select-String "nexusstack" | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
        } else {
            Write-Host "`n✗ 模板安装失败！" -ForegroundColor Red
        }
    }
    
    "2" {
        Write-Host "`n正在打包模板..." -ForegroundColor Green
        
        # 清理旧的包
        if (Test-Path ".\*.nupkg") {
            Remove-Item ".\*.nupkg" -Force
            Write-Host "已清理旧的 NuGet 包" -ForegroundColor Yellow
        }
        
        # 打包
        dotnet pack NexusStack.Template.csproj -c Release -o .
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "`n✓ 模板打包成功！" -ForegroundColor Green
            
            # 列出生成的包
            $packages = Get-ChildItem ".\*.nupkg"
            Write-Host "`n生成的 NuGet 包:" -ForegroundColor Cyan
            foreach ($pkg in $packages) {
                Write-Host "  $($pkg.Name)" -ForegroundColor White
            }
            
            Write-Host "`n安装此包:" -ForegroundColor Cyan
            Write-Host "  dotnet new install .\$($packages[0].Name)" -ForegroundColor White
        } else {
            Write-Host "`n✗ 模板打包失败！" -ForegroundColor Red
        }
    }
    
    "3" {
        Write-Host "`n准备发布到 NuGet.org..." -ForegroundColor Green
        
        # 打包
        Write-Host "正在打包..." -ForegroundColor Yellow
        dotnet pack NexusStack.Template.csproj -c Release -o .
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ 打包失败！" -ForegroundColor Red
            exit 1
        }
        
        # 获取生成的包
        $package = Get-ChildItem ".\*.nupkg" | Select-Object -First 1
        
        if (!$package) {
            Write-Host "✗ 未找到生成的 NuGet 包！" -ForegroundColor Red
            exit 1
        }
        
        Write-Host "`n找到包: $($package.Name)" -ForegroundColor Green
        
        # 询问 API Key
        Write-Host "`n请输入 NuGet.org API Key:" -ForegroundColor Yellow
        Write-Host "(如果没有，请访问 https://www.nuget.org/account/apikeys 创建)" -ForegroundColor Gray
        $apiKey = Read-Host -AsSecureString
        
        if (!$apiKey) {
            Write-Host "✗ 未提供 API Key！" -ForegroundColor Red
            exit 1
        }
        
        # 转换 SecureString 为纯文本
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($apiKey)
        $apiKeyText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
        
        # 确认发布
        Write-Host "`n准备将包发布到 NuGet.org" -ForegroundColor Yellow
        $confirm = Read-Host "确认发布? (Y/N)"
        
        if ($confirm -eq "Y" -or $confirm -eq "y") {
            Write-Host "正在发布..." -ForegroundColor Green
            dotnet nuget push ".\$($package.Name)" --api-key $apiKeyText --source https://api.nuget.org/v3/index.json
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "`n✓ 模板发布成功！" -ForegroundColor Green
                Write-Host "`n其他人可以通过以下命令安装:" -ForegroundColor Cyan
                Write-Host "  dotnet new install NexusStack.Template" -ForegroundColor White
            } else {
                Write-Host "`n✗ 模板发布失败！" -ForegroundColor Red
            }
        } else {
            Write-Host "已取消发布" -ForegroundColor Yellow
        }
        
        # 清理敏感信息
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    }
    
    "4" {
        Write-Host "`n正在卸载模板..." -ForegroundColor Green

        # 获取当前路径
        $currentPath = (Get-Location).Path

        Write-Host "尝试卸载模板..." -ForegroundColor Yellow
        Write-Host "  卸载路径: $currentPath" -ForegroundColor Gray

        # 尝试两种卸载方式（使用路径）
        $uninstalled = $false

        # 方式 1: 使用完整路径
        $result = dotnet new uninstall $currentPath 2>&1
        if ($LASTEXITCODE -eq 0) {
            $uninstalled = $true
            Write-Host "  使用完整路径卸载成功" -ForegroundColor Gray
        }

        # 方式 2: 使用相对路径
        if (-not $uninstalled) {
            $result = dotnet new uninstall . 2>&1
            if ($LASTEXITCODE -eq 0) {
                $uninstalled = $true
                Write-Host "  使用相对路径卸载成功" -ForegroundColor Gray
            }
        }

        if ($uninstalled) {
            Write-Host "`n✓ 模板卸载成功！" -ForegroundColor Green
        } else {
            Write-Host "`n⚠️ 未找到已安装的模板" -ForegroundColor Yellow
            Write-Host "`n提示: 查看所有已安装的模板及其卸载命令:" -ForegroundColor Cyan
            Write-Host "  dotnet new uninstall" -ForegroundColor White
            Write-Host ""
            dotnet new uninstall
        }
    }

    "5" {
        Write-Host "`n正在完全清理并重新安装模板..." -ForegroundColor Green

        # 获取当前路径
        $currentPath = (Get-Location).Path

        # 1. 卸载旧模板
        Write-Host "步骤 1/4: 卸载旧模板..." -ForegroundColor Yellow
        Write-Host "  卸载路径: $currentPath" -ForegroundColor Gray
        dotnet new uninstall $currentPath 2>$null
        dotnet new uninstall . 2>$null
        Write-Host "  旧模板已卸载" -ForegroundColor Gray

        # 2. 清理无效的模板引用
        Write-Host "步骤 2/4: 清理无效的模板引用..." -ForegroundColor Yellow
        $invalidPaths = @(
            "D:\Leo\TestTemplate",
            "D:\Leo\MineGuard_Backend"
        )
        foreach ($path in $invalidPaths) {
            try {
                dotnet new uninstall $path 2>$null | Out-Null
                Write-Host "  已移除引用: $path" -ForegroundColor Gray
            } catch {
                # 忽略错误
            }
        }

        # 3. 清理模板缓存
        Write-Host "步骤 3/5: 清理模板缓存..." -ForegroundColor Yellow
        $cacheDirs = @(
            "$env:USERPROFILE\.templateengine\dotnetcli",
            "$env:USERPROFILE\.templateengine"
        )
        foreach ($cacheDir in $cacheDirs) {
            if (Test-Path $cacheDir) {
                try {
                    Remove-Item -Recurse -Force $cacheDir -ErrorAction Stop
                    Write-Host "  已清理: $cacheDir" -ForegroundColor Gray
                } catch {
                    Write-Host "  清理失败: $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
        }
        Write-Host "  缓存清理完成" -ForegroundColor Gray

        # 4. 重新初始化
        Write-Host "步骤 4/5: 重新初始化模板引擎..." -ForegroundColor Yellow
        dotnet new --debug:reinit | Out-Null
        Write-Host "  模板引擎已重新初始化" -ForegroundColor Gray

        # 5. 安装新模板
        Write-Host "步骤 5/5: 安装模板..." -ForegroundColor Yellow
        dotnet new install .

        if ($LASTEXITCODE -eq 0) {
            Write-Host "`n✓ 模板完全清理并重新安装成功！" -ForegroundColor Green
            Write-Host "`n使用示例:" -ForegroundColor Cyan
            Write-Host "  dotnet new nexusstack -n MyProject" -ForegroundColor White
            Write-Host "`n查看详细参数:" -ForegroundColor Cyan
            Write-Host "  dotnet new nexusstack --help" -ForegroundColor White

            # 验证安装
            Write-Host "`n已安装的模板:" -ForegroundColor Cyan
            dotnet new list | Select-String "nexusstack" | ForEach-Object { Write-Host "  $_" -ForegroundColor White }
        } else {
            Write-Host "`n✗ 模板安装失败！" -ForegroundColor Red
            Write-Host "错误详情:" -ForegroundColor Yellow
            dotnet new install .
        }
    }

    default {
        Write-Host "`n无效的选项！" -ForegroundColor Red
        exit 1
    }
}

Write-Host "`n查看所有已安装的模板:" -ForegroundColor Cyan
Write-Host "  dotnet new list" -ForegroundColor White
Write-Host ""

# 防止窗口闪退
if ($choice -ne "") {
    Read-Host "`n按 Enter 键退出"
}
