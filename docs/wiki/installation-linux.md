# インストールガイド - オンプレミス Linux

このドキュメントでは、VehicleVision.Pleasanter.ReplicaSync をオンプレミスの Linux 環境にインストールする手順を説明します。
Debian / Ubuntu および AlmaLinux（RHEL 互換）の両方の手順を記載しています。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [前提条件](#前提条件)
- [手順 1: .NET 10 ランタイムのインストール](#手順-1-net-10-ランタイムのインストール)
    - [Debian / Ubuntu](#debian--ubuntu)
    - [AlmaLinux](#almalinux)
    - [インストール確認](#インストール確認)
- [手順 2: アプリケーションの取得](#手順-2-アプリケーションの取得)
    - [方法 A: NuGet パッケージ（推奨）](#方法-a-nuget-パッケージ推奨)
    - [方法 B: ソースコードからビルド](#方法-b-ソースコードからビルド)
- [手順 3: 構成データベースの準備](#手順-3-構成データベースの準備)
    - [PostgreSQL](#postgresql)
    - [MySQL](#mysql)
    - [SQL Server（Linux 版）](#sql-serverlinux-版)
- [手順 4: アプリケーションの配置](#手順-4-アプリケーションの配置)
- [手順 5: 設定ファイルの編集](#手順-5-設定ファイルの編集)
    - [Web UI（`/opt/replicasync/web/appsettings.json`）](#web-uioptreplicasyncwebappsettingsjson)
    - [Worker サービス（`/opt/replicasync/worker/appsettings.json`）](#worker-サービスoptreplicasyncworkerappsettingsjson)
    - [DBMS 別の接続文字列例](#dbms-別の接続文字列例)
- [手順 6: systemd サービスの登録](#手順-6-systemd-サービスの登録)
    - [Web UI サービス](#web-ui-サービス)
    - [Worker サービス](#worker-サービス)
    - [サービスの有効化と起動](#サービスの有効化と起動)
    - [サービスの管理](#サービスの管理)
- [手順 7: リバースプロキシの設定（推奨）](#手順-7-リバースプロキシの設定推奨)
    - [Nginx の場合](#nginx-の場合)
    - [HTTPS の設定（Let's Encrypt）](#https-の設定lets-encrypt)
- [手順 8: ファイアウォールの設定](#手順-8-ファイアウォールの設定)
    - [Debian / Ubuntu（ufw）](#debian--ubuntuufw)
    - [AlmaLinux（firewalld）](#almalinuxfirewalld)
    - [SELinux の設定（AlmaLinux）](#selinux-の設定almalinux)
- [手順 9: 動作確認](#手順-9-動作確認)
- [ディレクトリ構成（推奨）](#ディレクトリ構成推奨)
- [アップデート手順](#アップデート手順)
- [トラブルシューティング](#トラブルシューティング)
    - [サービスが起動しない](#サービスが起動しない)
    - [権限エラー](#権限エラー)
    - [Nginx 経由でアクセスできない](#nginx-経由でアクセスできない)
    - [ポート 5000 が使用済み](#ポート-5000-が使用済み)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 前提条件

| 要件            | 詳細                                         |
| --------------- | -------------------------------------------- |
| OS              | Debian 12+ / Ubuntu 22.04+ / AlmaLinux 9+    |
| .NET ランタイム | .NET 10 Runtime（ASP.NET Core Runtime 含む） |
| データベース    | SQL Server / PostgreSQL / MySQL のいずれか   |
| ネットワーク    | Pleasanter インスタンスの DB に接続可能      |
| ユーザー権限    | root または sudo 権限                        |

## 手順 1: .NET 10 ランタイムのインストール

### Debian / Ubuntu

```bash
# Microsoft パッケージリポジトリの登録
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# ランタイムのインストール
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-10.0
```

### AlmaLinux

```bash
# Microsoft パッケージリポジトリの登録
sudo rpm -Uvh https://packages.microsoft.com/config/rhel/9/packages-microsoft-prod.rpm

# ランタイムのインストール
sudo dnf install -y aspnetcore-runtime-10.0
```

### インストール確認

```bash
dotnet --list-runtimes
```

出力に以下が含まれていることを確認します。

```text
Microsoft.AspNetCore.App 10.0.x [/usr/share/dotnet/shared/Microsoft.AspNetCore.App]
Microsoft.NETCore.App 10.0.x [/usr/share/dotnet/shared/Microsoft.NETCore.App]
```

## 手順 2: アプリケーションの取得

アプリケーションの取得方法は、NuGet パッケージとソースコードからのビルドの 2 つから選択できます。

### 方法 A: NuGet パッケージ（推奨）

NuGet パッケージを使用すると、ソースコードのクローンが不要です。詳細は [NuGet パッケージによるインストール](installation-nuget.md) を参照してください。

### 方法 B: ソースコードからビルド

ビルド環境で以下を実行します。ビルドには .NET 10 SDK が必要です。

```bash
# リポジトリのクローン
git clone https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ReplicaSync.git
cd VehicleVision.Pleasanter.ReplicaSync

# Web UI のビルド
dotnet publish src/VehicleVision.Pleasanter.ReplicaSync.Web/ReplicaSync.Web.csproj \
  --configuration Release \
  --output ./publish/web

# Worker サービスのビルド
dotnet publish src/VehicleVision.Pleasanter.ReplicaSync.Worker/ReplicaSync.Worker.csproj \
  --configuration Release \
  --output ./publish/worker
```

## 手順 3: 構成データベースの準備

構成データベースを作成します。アプリケーション起動時に EF Core がテーブルを自動作成するため、空のデータベースを用意するだけで構いません。

### PostgreSQL

```bash
sudo -u postgres psql -c "CREATE DATABASE replicasync;"
sudo -u postgres psql -c "CREATE USER replicauser WITH PASSWORD '<パスワード>';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE replicasync TO replicauser;"
```

### MySQL

```bash
sudo mysql -e "CREATE DATABASE replicasync CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
sudo mysql -e "CREATE USER 'replicauser'@'localhost' IDENTIFIED BY '<パスワード>';"
sudo mysql -e "GRANT ALL PRIVILEGES ON replicasync.* TO 'replicauser'@'localhost';"
sudo mysql -e "FLUSH PRIVILEGES;"
```

### SQL Server（Linux 版）

```bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P '<パスワード>' -Q "CREATE DATABASE ReplicaSync;"
```

## 手順 4: アプリケーションの配置

```bash
# 実行ユーザーの作成
sudo useradd --system --no-create-home --shell /usr/sbin/nologin replicasync

# 配置先ディレクトリの作成
sudo mkdir -p /opt/replicasync/web
sudo mkdir -p /opt/replicasync/worker

# ファイルのコピー
sudo cp -r ./publish/web/* /opt/replicasync/web/
sudo cp -r ./publish/worker/* /opt/replicasync/worker/

# 所有者の設定
sudo chown -R replicasync:replicasync /opt/replicasync
```

## 手順 5: 設定ファイルの編集

### Web UI（`/opt/replicasync/web/appsettings.json`）

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "Host=localhost;Database=replicasync;Username=replicauser;Password=<パスワード>;"
    },
    "ConfigDatabaseType": "PostgreSql"
}
```

### Worker サービス（`/opt/replicasync/worker/appsettings.json`）

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "Host=localhost;Database=replicasync;Username=replicauser;Password=<パスワード>;"
    },
    "ConfigDatabaseType": "PostgreSql"
}
```

> **重要**: Web と Worker は同じ構成データベースを参照するように設定してください。

### DBMS 別の接続文字列例

#### SQL Server

```text
Server=localhost;Database=ReplicaSync;User ID=sa;Password=<パスワード>;TrustServerCertificate=True;
```

#### MySQL

```text
Server=localhost;Database=replicasync;User=replicauser;Password=<パスワード>;
```

## 手順 6: systemd サービスの登録

### Web UI サービス

```bash
sudo tee /etc/systemd/system/replicasync-web.service > /dev/null << 'EOF'
[Unit]
Description=ReplicaSync Web UI
After=network.target

[Service]
Type=notify
User=replicasync
Group=replicasync
WorkingDirectory=/opt/replicasync/web
ExecStart=/usr/bin/dotnet /opt/replicasync/web/ReplicaSync.Web.dll --urls "http://0.0.0.0:5000"
Restart=always
RestartSec=10
SyslogIdentifier=replicasync-web

# セキュリティ強化
ProtectSystem=full
ProtectHome=true
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF
```

### Worker サービス

```bash
sudo tee /etc/systemd/system/replicasync-worker.service > /dev/null << 'EOF'
[Unit]
Description=ReplicaSync Worker Service
After=network.target

[Service]
Type=notify
User=replicasync
Group=replicasync
WorkingDirectory=/opt/replicasync/worker
ExecStart=/usr/bin/dotnet /opt/replicasync/worker/ReplicaSync.Worker.dll
Restart=always
RestartSec=10
SyslogIdentifier=replicasync-worker

# セキュリティ強化
ProtectSystem=full
ProtectHome=true
NoNewPrivileges=true
PrivateTmp=true

[Install]
WantedBy=multi-user.target
EOF
```

### サービスの有効化と起動

```bash
# systemd の再読み込み
sudo systemctl daemon-reload

# サービスの有効化（OS 起動時に自動起動）
sudo systemctl enable replicasync-web
sudo systemctl enable replicasync-worker

# サービスの開始
sudo systemctl start replicasync-web
sudo systemctl start replicasync-worker
```

### サービスの管理

```bash
# ステータス確認
sudo systemctl status replicasync-web
sudo systemctl status replicasync-worker

# 停止
sudo systemctl stop replicasync-web
sudo systemctl stop replicasync-worker

# 再起動
sudo systemctl restart replicasync-web
sudo systemctl restart replicasync-worker

# ログの確認（journalctl）
sudo journalctl -u replicasync-worker -f
sudo journalctl -u replicasync-web -f
```

## 手順 7: リバースプロキシの設定（推奨）

本番環境では、Kestrel の前段にリバースプロキシを配置することを推奨します。

### Nginx の場合

#### インストール

```bash
# Debian / Ubuntu
sudo apt-get install -y nginx

# AlmaLinux
sudo dnf install -y nginx
```

#### 設定ファイルの作成

```bash
sudo tee /etc/nginx/sites-available/replicasync > /dev/null << 'EOF'
server {
    listen 80;
    server_name replicasync.example.com;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;

        # Blazor Server の SignalR 接続用
        proxy_read_timeout 86400;
    }
}
EOF
```

#### 有効化

```bash
# Debian / Ubuntu
sudo ln -s /etc/nginx/sites-available/replicasync /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default

# AlmaLinux（conf.d を使用）
sudo cp /etc/nginx/sites-available/replicasync /etc/nginx/conf.d/replicasync.conf

# 設定の検証と再読み込み
sudo nginx -t
sudo systemctl reload nginx
```

> **注意**: AlmaLinux では `/etc/nginx/sites-available/` ディレクトリがデフォルトで存在しない場合があります。その場合は `/etc/nginx/conf.d/` に直接設定ファイルを配置してください。

### HTTPS の設定（Let's Encrypt）

```bash
# Debian / Ubuntu
sudo apt-get install -y certbot python3-certbot-nginx

# AlmaLinux
sudo dnf install -y certbot python3-certbot-nginx

# 証明書の取得と自動設定
sudo certbot --nginx -d replicasync.example.com
```

## 手順 8: ファイアウォールの設定

### Debian / Ubuntu（ufw）

```bash
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw reload
```

### AlmaLinux（firewalld）

```bash
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --reload
```

### SELinux の設定（AlmaLinux）

AlmaLinux では SELinux がデフォルトで有効なため、Nginx からのプロキシ接続を許可します。

```bash
sudo setsebool -P httpd_can_network_connect 1
```

## 手順 9: 動作確認

1. Web UI にブラウザでアクセスします

    ```text
    http://<サーバーのIPアドレスまたはホスト名>
    ```

2. 管理画面から同期インスタンスと同期定義を登録します
3. Worker サービスのログを確認します

    ```bash
    # journalctl でリアルタイムログを表示
    sudo journalctl -u replicasync-worker -f

    # NLog のログファイルを確認
    tail -f /opt/replicasync/worker/logs/replicasync-worker-*.log
    ```

## ディレクトリ構成（推奨）

```text
/opt/replicasync/
├── web/                              # Web UI
│   ├── ReplicaSync.Web.dll
│   ├── appsettings.json
│   ├── nlog.config
│   ├── wwwroot/
│   └── logs/                         # ログファイル出力先
└── worker/                           # Worker サービス
    ├── ReplicaSync.Worker.dll
    ├── appsettings.json
    ├── nlog.config
    └── logs/                         # ログファイル出力先
```

## アップデート手順

1. サービスを停止します

    ```bash
    sudo systemctl stop replicasync-worker
    sudo systemctl stop replicasync-web
    ```

2. 既存ファイルをバックアップします

    ```bash
    sudo cp -r /opt/replicasync/web /opt/replicasync/web-backup-$(date +%Y%m%d)
    sudo cp -r /opt/replicasync/worker /opt/replicasync/worker-backup-$(date +%Y%m%d)
    ```

3. 新しいビルド成果物を配置します（`appsettings.json` は上書きしないよう注意）

    ```bash
    # appsettings.json を退避
    cp /opt/replicasync/web/appsettings.json /tmp/web-appsettings.json
    cp /opt/replicasync/worker/appsettings.json /tmp/worker-appsettings.json

    # 新しいファイルを配置
    sudo cp -r ./publish/web/* /opt/replicasync/web/
    sudo cp -r ./publish/worker/* /opt/replicasync/worker/

    # appsettings.json を復元
    sudo cp /tmp/web-appsettings.json /opt/replicasync/web/appsettings.json
    sudo cp /tmp/worker-appsettings.json /opt/replicasync/worker/appsettings.json

    # 所有者の再設定
    sudo chown -R replicasync:replicasync /opt/replicasync
    ```

4. サービスを再開します

    ```bash
    sudo systemctl start replicasync-web
    sudo systemctl start replicasync-worker
    ```

## トラブルシューティング

### サービスが起動しない

```bash
# 詳細なステータス確認
sudo systemctl status replicasync-worker -l

# journalctl で直近のログを確認
sudo journalctl -u replicasync-worker --since "5 minutes ago"

# 手動起動でエラーを確認
sudo -u replicasync dotnet /opt/replicasync/worker/ReplicaSync.Worker.dll
```

### 権限エラー

- ファイルの所有者が `replicasync` ユーザーであることを確認します

    ```bash
    ls -la /opt/replicasync/
    ```

- ログ出力先ディレクトリの書き込み権限を確認します

    ```bash
    sudo mkdir -p /opt/replicasync/worker/logs
    sudo chown replicasync:replicasync /opt/replicasync/worker/logs
    ```

### Nginx 経由でアクセスできない

- Nginx の設定が正しいか確認します

    ```bash
    sudo nginx -t
    ```

- Kestrel が起動しているか確認します

    ```bash
    curl http://127.0.0.1:5000
    ```

- AlmaLinux の場合、SELinux が原因の可能性があります

    ```bash
    sudo ausearch -m avc --ts recent
    sudo setsebool -P httpd_can_network_connect 1
    ```

### ポート 5000 が使用済み

別のポートを使用する場合は systemd のサービスファイルの `ExecStart` を修正します。

```bash
ExecStart=/usr/bin/dotnet /opt/replicasync/web/ReplicaSync.Web.dll --urls "http://0.0.0.0:8080"
```

変更後にサービスを再読み込みします。

```bash
sudo systemctl daemon-reload
sudo systemctl restart replicasync-web
```
