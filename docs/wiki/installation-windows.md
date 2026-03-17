# インストールガイド - オンプレミス Windows

このドキュメントでは、VehicleVision.Pleasanter.ReplicaSync をオンプレミスの Windows 環境にインストールする手順を説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [前提条件](#前提条件)
- [手順 1: .NET 10 ランタイムのインストール](#手順-1-net-10-ランタイムのインストール)
    - [ダウンロード](#ダウンロード)
    - [インストール確認](#インストール確認)
- [手順 2: アプリケーションの取得](#手順-2-アプリケーションの取得)
    - [方法 A: NuGet パッケージ（推奨）](#方法-a-nuget-パッケージ推奨)
    - [方法 B: ソースコードからビルド](#方法-b-ソースコードからビルド)
- [手順 3: 構成データベースの準備](#手順-3-構成データベースの準備)
    - [SQL Server](#sql-server)
    - [PostgreSQL](#postgresql)
    - [MySQL](#mysql)
- [手順 4: アプリケーションの配置](#手順-4-アプリケーションの配置)
- [手順 5: 設定ファイルの編集](#手順-5-設定ファイルの編集)
    - [Web UI（`C:\ReplicaSync\web\appsettings.json`）](#web-uicreplicasyncwebappsettingsjson)
    - [Worker サービス（`C:\ReplicaSync\worker\appsettings.json`）](#worker-サービスcreplicasyncworkerappsettingsjson)
    - [DBMS 別の接続文字列例](#dbms-別の接続文字列例)
- [手順 6: Web UI のセットアップ](#手順-6-web-ui-のセットアップ)
    - [方法 A: IIS でホスティング](#方法-a-iis-でホスティング)
    - [方法 B: スタンドアロン（Kestrel）で実行](#方法-b-スタンドアロンkestrelで実行)
- [手順 7: Worker サービスの Windows サービス登録](#手順-7-worker-サービスの-windows-サービス登録)
    - [sc コマンドで登録](#sc-コマンドで登録)
    - [サービスの管理](#サービスの管理)
- [手順 8: ファイアウォールの設定](#手順-8-ファイアウォールの設定)
- [手順 9: 動作確認](#手順-9-動作確認)
- [ディレクトリ構成（推奨）](#ディレクトリ構成推奨)
- [アップデート手順](#アップデート手順)
- [トラブルシューティング](#トラブルシューティング)
    - [Web UI にアクセスできない](#web-ui-にアクセスできない)
    - [Worker サービスが起動しない](#worker-サービスが起動しない)
    - [データベース接続エラー](#データベース接続エラー)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 前提条件

| 要件            | 詳細                                         |
| --------------- | -------------------------------------------- |
| OS              | Windows Server 2019 以降 / Windows 10 以降   |
| .NET ランタイム | .NET 10 Runtime（ASP.NET Core Runtime 含む） |
| データベース    | SQL Server / PostgreSQL / MySQL のいずれか   |
| ネットワーク    | Pleasanter インスタンスの DB に接続可能      |

## 手順 1: .NET 10 ランタイムのインストール

### ダウンロード

[.NET 10 ダウンロードページ](https://dotnet.microsoft.com/download/dotnet/10.0) から以下をダウンロードします。

- **ASP.NET Core Runtime 10.0.x** — Web UI（ReplicaSync.Web）の実行に必要
- **.NET Runtime 10.0.x** — Worker サービス（ReplicaSync.Worker）の実行に必要

> **ヒント**: 「Hosting Bundle」をインストールすると、ASP.NET Core Runtime と IIS モジュールがまとめてインストールされます。

### インストール確認

```powershell
dotnet --list-runtimes
```

出力に以下が含まれていることを確認します。

```text
Microsoft.AspNetCore.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
Microsoft.NETCore.App 10.0.x [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
```

## 手順 2: アプリケーションの取得

アプリケーションの取得方法は、NuGet パッケージとソースコードからのビルドの 2 つから選択できます。

### 方法 A: NuGet パッケージ（推奨）

NuGet パッケージを使用すると、ソースコードのクローンが不要です。詳細は [NuGet パッケージによるインストール](installation-nuget.md) を参照してください。

### 方法 B: ソースコードからビルド

ビルド環境（または開発端末）で以下を実行します。

```powershell
# リポジトリのクローン
git clone https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ReplicaSync.git
cd VehicleVision.Pleasanter.ReplicaSync

# Web UI のビルド
dotnet publish src\ReplicaSync.Web\ReplicaSync.Web.csproj `
  --configuration Release `
  --output .\publish\web

# Worker サービスのビルド
dotnet publish src\ReplicaSync.Worker\ReplicaSync.Worker.csproj `
  --configuration Release `
  --output .\publish\worker
```

## 手順 3: 構成データベースの準備

構成データベースを作成します。アプリケーション起動時に EF Core がテーブルを自動作成するため、空のデータベースを用意するだけで構いません。

### SQL Server

```sql
CREATE DATABASE ReplicaSync;
```

### PostgreSQL

```sql
CREATE DATABASE replicasync;
```

### MySQL

```sql
CREATE DATABASE replicasync CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

## 手順 4: アプリケーションの配置

ビルド成果物を運用サーバーにコピーします。

```powershell
# 配置先ディレクトリの作成
New-Item -ItemType Directory -Path C:\ReplicaSync\web -Force
New-Item -ItemType Directory -Path C:\ReplicaSync\worker -Force

# ファイルのコピー
Copy-Item -Path .\publish\web\* -Destination C:\ReplicaSync\web -Recurse
Copy-Item -Path .\publish\worker\* -Destination C:\ReplicaSync\worker -Recurse
```

## 手順 5: 設定ファイルの編集

### Web UI（`C:\ReplicaSync\web\appsettings.json`）

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "Server=localhost;Database=ReplicaSync;Trusted_Connection=True;TrustServerCertificate=True;"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

### Worker サービス（`C:\ReplicaSync\worker\appsettings.json`）

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "Server=localhost;Database=ReplicaSync;Trusted_Connection=True;TrustServerCertificate=True;"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

> **重要**: Web と Worker は同じ構成データベースを参照するように設定してください。

### DBMS 別の接続文字列例

#### PostgreSQL

```text
Host=localhost;Database=replicasync;Username=pguser;Password=<パスワード>;
```

#### MySQL

```text
Server=localhost;Database=replicasync;User=mysqluser;Password=<パスワード>;
```

## 手順 6: Web UI のセットアップ

Web UI の実行方法は、IIS を使用する方法とスタンドアロンで実行する方法の 2 通りがあります。

### 方法 A: IIS でホスティング

#### 前提

- IIS がインストールされていること
- ASP.NET Core Hosting Bundle がインストールされていること

#### IIS サイトの作成

1. IIS マネージャーを開きます
2. **サイト** を右クリック → **Web サイトの追加**
3. 以下を設定します

    | 設定項目 | 値                         |
    | -------- | -------------------------- |
    | サイト名 | ReplicaSync-Web            |
    | 物理パス | `C:\ReplicaSync\web`       |
    | バインド | http / ポート 5000（任意） |

4. アプリケーションプールの設定

    | 設定項目                       | 値                    |
    | ------------------------------ | --------------------- |
    | .NET CLR バージョン            | マネージド コードなし |
    | マネージド パイプライン モード | 統合                  |

#### web.config の確認

`publish` 時に自動生成される `web.config` が配置先に存在することを確認します。

### 方法 B: スタンドアロン（Kestrel）で実行

```powershell
cd C:\ReplicaSync\web
dotnet ReplicaSync.Web.dll --urls "http://0.0.0.0:5000"
```

## 手順 7: Worker サービスの Windows サービス登録

Worker をバックグラウンドで常時稼働させるため、Windows サービスとして登録します。

### sc コマンドで登録

```powershell
sc.exe create ReplicaSyncWorker `
  binpath= "C:\ReplicaSync\worker\ReplicaSync.Worker.exe" `
  start= auto `
  DisplayName= "ReplicaSync Worker Service"

# サービスの説明を設定
sc.exe description ReplicaSyncWorker "VehicleVision.Pleasanter.ReplicaSync の同期ワーカーサービス"

# サービスの開始
sc.exe start ReplicaSyncWorker
```

### サービスの管理

```powershell
# ステータス確認
sc.exe query ReplicaSyncWorker

# 停止
sc.exe stop ReplicaSyncWorker

# 削除（アンインストール時）
sc.exe delete ReplicaSyncWorker
```

> **注意**: Windows サービスとして実行する場合、作業ディレクトリが `C:\Windows\System32` になることがあります。`nlog.config` のログ出力先が正しく解決されるよう、絶対パスの指定を検討してください。

## 手順 8: ファイアウォールの設定

Web UI にネットワーク内の他のマシンからアクセスする場合は、Windows ファイアウォールでポートを開放します。

```powershell
New-NetFirewallRule `
  -DisplayName "ReplicaSync Web UI" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 5000 `
  -Action Allow
```

## 手順 9: 動作確認

1. Web UI にブラウザでアクセスします

    ```text
    http://localhost:5000
    ```

2. 管理画面から同期インスタンスと同期定義を登録します
3. Worker サービスのログを確認します

    ```powershell
    # ログファイルの確認
    Get-Content C:\ReplicaSync\worker\logs\replicasync-worker-*.log -Tail 50
    ```

## ディレクトリ構成（推奨）

```text
C:\ReplicaSync\
├── web\                          # Web UI
│   ├── ReplicaSync.Web.dll
│   ├── appsettings.json
│   ├── nlog.config
│   ├── web.config                # IIS 使用時
│   ├── wwwroot\
│   └── logs\                     # ログファイル出力先
├── worker\                       # Worker サービス
│   ├── ReplicaSync.Worker.exe
│   ├── ReplicaSync.Worker.dll
│   ├── appsettings.json
│   ├── nlog.config
│   └── logs\                     # ログファイル出力先
└── backup\                       # バックアップ用（任意）
```

## アップデート手順

1. Worker サービスを停止します

    ```powershell
    sc.exe stop ReplicaSyncWorker
    ```

2. IIS サイトを停止します（IIS 使用時）

    ```powershell
    Stop-WebSite -Name "ReplicaSync-Web"
    ```

3. 既存ファイルをバックアップします

    ```powershell
    Copy-Item -Path C:\ReplicaSync\web -Destination C:\ReplicaSync\backup\web-$(Get-Date -Format yyyyMMdd) -Recurse
    Copy-Item -Path C:\ReplicaSync\worker -Destination C:\ReplicaSync\backup\worker-$(Get-Date -Format yyyyMMdd) -Recurse
    ```

4. 新しいビルド成果物を配置します（`appsettings.json` は上書きしないよう注意）
5. サービスを再開します

    ```powershell
    sc.exe start ReplicaSyncWorker
    Start-WebSite -Name "ReplicaSync-Web"  # IIS 使用時
    ```

## トラブルシューティング

### Web UI にアクセスできない

- .NET ランタイムがインストールされているか確認してください
- IIS 使用時は ASP.NET Core Hosting Bundle がインストールされているか確認してください
- ファイアウォールのポート開放を確認してください
- `C:\ReplicaSync\web\logs\` のログファイルでエラーを確認してください

### Worker サービスが起動しない

- イベントビューアー（Windows ログ → Application）でエラーを確認します
- `C:\ReplicaSync\worker\logs\` のログファイルを確認します
- 構成データベースへの接続文字列が正しいか確認します

### データベース接続エラー

- 接続文字列のサーバー名・ポートが正しいか確認します
- Windows 認証（`Trusted_Connection=True`）を使用する場合、サービスの実行アカウントにデータベースへのアクセス権があることを確認します
- SQL 認証を使用する場合、ユーザー名・パスワードが正しいか確認します
