# VehicleVision.Pleasanter.ReplicaSync

<!-- markdownlint-disable MD013 -->

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/) [![Pleasanter](https://img.shields.io/badge/Pleasanter-1.3.13.0%2B-00A0E9)](https://pleasanter.org/) [![License](https://img.shields.io/badge/License-LGPL--2.1-blue.svg)](LICENSE) [![NuGet](https://img.shields.io/nuget/v/ReplicaSync.Web.svg?logo=nuget)](https://www.nuget.org/packages/ReplicaSync.Web)

<!-- markdownlint-enable MD013 -->

複数の Pleasanter インスタンス間でデータを同期するためのレプリカ同期プラットフォームです。
Hub-Spoke / Peer-to-Peer トポロジ、複数の競合解決戦略、マルチDBMS（SQL Server / PostgreSQL / MySQL）をサポートします。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [主な機能](#主な機能)
- [クイックスタート](#クイックスタート)
    - [前提条件](#前提条件)
    - [インストール](#インストール)
    - [設定](#設定)
    - [起動](#起動)
- [ドキュメント](#ドキュメント)
- [NuGet パッケージ](#nuget-パッケージ)
- [サードパーティライセンス](#サードパーティライセンス)
- [セキュリティ](#セキュリティ)
- [開発に参加する](#開発に参加する)
- [謝辞](#謝辞)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 主な機能

- **マルチトポロジ対応** - Hub-Spoke（親子）および Peer-to-Peer トポロジをサポート
- **競合解決戦略** - SourceWins / LastWriteWins / ManualResolution / FieldLevelMerge の4つの戦略から選択
- **カラムレベル制御** - 同期対象カラムの Include / Exclude フィルタリング
- **マルチDBMS** - SQL Server、PostgreSQL、MySQL の Pleasanter インスタンスおよび構成データベースに対応
- **同期キー** - 任意のカラムをキーとしたレコードマッチング
- **削除同期** - ソフトデリート追跡による削除レコードの同期
- **バージョン履歴** - 上書き前のレコード状態をスナップショットとして自動保存
- **監査ログ** - すべての同期操作を詳細に記録
- **Web UI** - Blazor Server ベースの管理画面でインスタンス・定義・ログを管理
- **Web API** - API キー認証による REST API で外部システムとの連携が可能
- **バックグラウンドサービス** - .NET Worker Service による継続的なポーリング同期

## クイックスタート

### 前提条件

- [.NET 10 ランタイム](https://dotnet.microsoft.com/download)（ASP.NET Core Runtime 含む）
- SQL Server / PostgreSQL / MySQL のいずれか（構成データベース用）
- 同期対象の Pleasanter インスタンスのデータベースへの接続

### インストール

NuGet パッケージを使用して、最も簡単にセットアップできます。

```bash
# Web UI（管理画面）
dotnet new web -n ReplicaSyncWeb
cd ReplicaSyncWeb
dotnet add package ReplicaSync.Web

# Worker サービス（同期実行）
cd ..
dotnet new worker -n ReplicaSyncWorker
cd ReplicaSyncWorker
dotnet add package ReplicaSync.Worker
```

詳細な手順は[インストールガイド](docs/wiki/Home.md#インストールガイド)を参照してください。

### 設定

`appsettings.json` で構成データベースの接続先を設定します。

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "<構成データベースの接続文字列>"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

`ConfigDatabaseType` には `SqlServer`、`PostgreSql`、`MySql` を指定できます。
詳細は[設定ガイド](docs/wiki/configuration-guide.md)を参照してください。

### 起動

```bash
# Web UI を起動（ブラウザで管理画面にアクセス）
dotnet run --project ReplicaSyncWeb

# Worker サービスを起動（バックグラウンド同期を実行）
dotnet run --project ReplicaSyncWorker
```

Web UI のダッシュボードから、以下の操作が可能です：

- 同期インスタンスの登録・編集・削除
- 同期定義の作成・有効化・無効化
- 同期ログの確認
- ユーザー管理（管理者のみ）

操作方法の詳細は [Web UI 取扱説明書](docs/wiki/web-manual.md)を参照してください。

## ドキュメント

詳細なドキュメントは [Wiki](docs/wiki/Home.md) にまとめています。

<!-- markdownlint-disable MD013 MD060 -->

| カテゴリ     | ドキュメント                                                                                                                                                                  |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| インストール | [NuGet](docs/wiki/installation-nuget.md) / [Azure](docs/wiki/installation-azure.md) / [Windows](docs/wiki/installation-windows.md) / [Linux](docs/wiki/installation-linux.md) |
| 設定         | [設定ガイド](docs/wiki/configuration-guide.md)                                                                                                                                |
| 操作方法     | [Web UI 取扱説明書](docs/wiki/web-manual.md)                                                                                                                                  |
| API          | [Web API リファレンス](docs/wiki/web-api-reference.md)                                                                                                                        |

<!-- markdownlint-enable MD013 MD060 -->

## NuGet パッケージ

<!-- markdownlint-disable MD013 -->

| パッケージ                                                              | 説明                                 |
| ----------------------------------------------------------------------- | ------------------------------------ |
| [ReplicaSync.Web](https://www.nuget.org/packages/ReplicaSync.Web)       | Blazor Server ベースの管理 Web UI    |
| [ReplicaSync.Worker](https://www.nuget.org/packages/ReplicaSync.Worker) | バックグラウンド同期 Worker サービス |

<!-- markdownlint-enable MD013 -->

```bash
dotnet add package ReplicaSync.Web
dotnet add package ReplicaSync.Worker
```

## サードパーティライセンス

このプロジェクトは以下のサードパーティライブラリを使用しています：

<!-- markdownlint-disable MD013 -->

| ライブラリ                              | ライセンス   | 用途                            |
| --------------------------------------- | ------------ | ------------------------------- |
| Microsoft.EntityFrameworkCore           | MIT          | ORM / 構成データベースアクセス  |
| Microsoft.EntityFrameworkCore.SqlServer | MIT          | SQL Server プロバイダー         |
| Microsoft.Extensions.Hosting            | MIT          | Worker サービスホスティング     |
| Microsoft.Extensions.Logging            | MIT          | ログ抽象化                      |
| NLog                                    | BSD-3-Clause | ロギングフレームワーク          |
| NLog.Extensions.Hosting                 | BSD-3-Clause | NLog の .NET Host 統合          |
| NLog.Web.AspNetCore                     | BSD-3-Clause | NLog の ASP.NET Core 統合       |
| Microsoft.Data.SqlClient                | MIT          | SQL Server ADO.NET ドライバー   |
| Npgsql                                  | PostgreSQL   | PostgreSQL ADO.NET ドライバー   |
| Npgsql.EntityFrameworkCore.PostgreSQL   | PostgreSQL   | PostgreSQL EF Core プロバイダー |
| MySqlConnector                          | MIT          | MySQL ADO.NET ドライバー        |
| Microting.EntityFrameworkCore.MySql     | MIT          | MySQL EF Core プロバイダー      |

<!-- markdownlint-enable MD013 -->

ライセンスファイルの全文は [LICENSES](./LICENSES/) フォルダを参照してください。

## セキュリティ

セキュリティ上の脆弱性を発見された場合は、[セキュリティポリシー](.github/SECURITY.md)をご確認の上、ご報告ください。

## 開発に参加する

プロジェクトへの貢献を歓迎します。バグ報告、機能要望、プルリクエストについては[コントリビューションガイド](CONTRIBUTING.md)をご覧ください。

## 謝辞

セキュリティ脆弱性の報告やプロジェクトへの貢献をしてくださった方々に感謝いたします。

<!-- 貢献者・報告者はこちらに追記 -->
