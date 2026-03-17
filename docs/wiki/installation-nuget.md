# インストールガイド - NuGet パッケージ

このドキュメントでは、NuGet パッケージを使用して ReplicaSync をセットアップする手順を説明します。
ソースコードからのビルドが不要な、最も簡単なインストール方法です。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [前提条件](#前提条件)
- [概要](#概要)
- [手順 1: プロジェクトの作成](#手順-1-プロジェクトの作成)
    - [Web UI](#web-ui)
    - [Worker サービス](#worker-サービス)
- [手順 2: 構成データベースの準備](#手順-2-構成データベースの準備)
    - [SQL Server](#sql-server)
    - [PostgreSQL](#postgresql)
    - [MySQL](#mysql)
- [手順 3: 設定ファイルの編集](#手順-3-設定ファイルの編集)
    - [Web UI（`appsettings.json`）](#web-uiappsettingsjson)
    - [Worker サービス（`appsettings.json`）](#worker-サービスappsettingsjson)
    - [DBMS 別の接続文字列例](#dbms-別の接続文字列例)
- [手順 4: アプリケーションの発行](#手順-4-アプリケーションの発行)
- [手順 5: 動作確認](#手順-5-動作確認)
    - [Web UI の起動](#web-ui-の起動)
    - [Worker サービスの起動](#worker-サービスの起動)
- [NuGet パッケージの更新](#nuget-パッケージの更新)
- [デプロイ先別の詳細手順](#デプロイ先別の詳細手順)
- [トラブルシューティング](#トラブルシューティング)
    - [パッケージが見つからない](#パッケージが見つからない)
    - [バージョンの互換性エラー](#バージョンの互換性エラー)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 前提条件

| 要件         | 詳細                                       |
| ------------ | ------------------------------------------ |
| .NET SDK     | .NET 10 SDK                                |
| データベース | SQL Server / PostgreSQL / MySQL のいずれか |
| ネットワーク | Pleasanter インスタンスの DB に接続可能    |

## 概要

NuGet パッケージとして公開されている以下の 2 つのパッケージを使用します。

<!-- markdownlint-disable MD013 -->

| パッケージ                                                              | 説明                                 |
| ----------------------------------------------------------------------- | ------------------------------------ |
| [VehicleVision.Pleasanter.ReplicaSync.Web](https://www.nuget.org/packages/VehicleVision.Pleasanter.ReplicaSync.Web)       | Blazor Server ベースの管理 Web UI    |
| [VehicleVision.Pleasanter.ReplicaSync.Worker](https://www.nuget.org/packages/VehicleVision.Pleasanter.ReplicaSync.Worker) | バックグラウンド同期 Worker サービス |

<!-- markdownlint-enable MD013 -->

## 手順 1: プロジェクトの作成

### Web UI

```bash
mkdir ReplicaSyncWeb && cd ReplicaSyncWeb
dotnet new web
dotnet add package VehicleVision.Pleasanter.ReplicaSync.Web
```

### Worker サービス

```bash
mkdir ReplicaSyncWorker && cd ReplicaSyncWorker
dotnet new worker
dotnet add package VehicleVision.Pleasanter.ReplicaSync.Worker
```

## 手順 2: 構成データベースの準備

アプリケーション起動時に EF Core がテーブルを自動作成するため、空のデータベースを用意するだけで構いません。

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

## 手順 3: 設定ファイルの編集

### Web UI（`appsettings.json`）

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "Server=localhost;Database=ReplicaSync;Trusted_Connection=True;TrustServerCertificate=True;"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

### Worker サービス（`appsettings.json`）

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

## 手順 4: アプリケーションの発行

```bash
# Web UI の発行
cd ReplicaSyncWeb
dotnet publish --configuration Release --output ./publish

# Worker サービスの発行
cd ReplicaSyncWorker
dotnet publish --configuration Release --output ./publish
```

## 手順 5: 動作確認

### Web UI の起動

```bash
cd ReplicaSyncWeb/publish
dotnet ReplicaSyncWeb.dll --urls "http://0.0.0.0:5000"
```

ブラウザで `http://localhost:5000` にアクセスし、管理画面が表示されることを確認します。

### Worker サービスの起動

```bash
cd ReplicaSyncWorker/publish
dotnet ReplicaSyncWorker.dll
```

## NuGet パッケージの更新

新しいバージョンがリリースされた場合は、以下のコマンドで更新できます。

```bash
dotnet add package VehicleVision.Pleasanter.ReplicaSync.Web
dotnet add package VehicleVision.Pleasanter.ReplicaSync.Worker
```

特定のバージョンを指定する場合：

```bash
dotnet add package VehicleVision.Pleasanter.ReplicaSync.Web --version <バージョン>
dotnet add package VehicleVision.Pleasanter.ReplicaSync.Worker --version <バージョン>
```

更新後に再発行してデプロイしてください。

```bash
dotnet publish --configuration Release --output ./publish
```

## デプロイ先別の詳細手順

NuGet パッケージでアプリケーションを発行した後のデプロイ方法は、環境に応じて以下のガイドを参照してください。

- [オンプレミス Windows](installation-windows.md) — 手順 4 以降（配置・IIS・Windows サービス登録）
- [オンプレミス Linux](installation-linux.md) — 手順 4 以降（配置・systemd 登録）
- [Azure 環境](installation-azure.md) — 手順 5 以降（App Service へのデプロイ）

## トラブルシューティング

### パッケージが見つからない

- NuGet.org がソースとして登録されているか確認してください

    ```bash
    dotnet nuget list source
    ```

- プロキシ環境の場合はプロキシ設定を確認してください

### バージョンの互換性エラー

- .NET 10 SDK がインストールされているか確認してください

    ```bash
    dotnet --version
    ```

- プロジェクトの `TargetFramework` が `net10.0` であることを確認してください
