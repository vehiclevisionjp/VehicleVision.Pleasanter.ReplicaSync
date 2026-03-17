# VehicleVision.Pleasanter.ReplicaSync

<!-- markdownlint-disable MD013 -->

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/) [![Pleasanter](https://img.shields.io/badge/Pleasanter-1.3.13.0%2B-00A0E9)](https://pleasanter.org/) [![License](https://img.shields.io/badge/License-LGPL--2.1-blue.svg)](LICENSE)

<!-- markdownlint-enable MD013 -->

複数の Pleasanter インスタンス間でデータを同期するためのレプリカ同期プラットフォームです。
Hub-Spoke / Peer-to-Peer トポロジ、複数の競合解決戦略、マルチDBMS（SQL Server / PostgreSQL / MySQL）をサポートします。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [VehicleVision.Pleasanter.ReplicaSync](#vehiclevisionpleasanterreplicasync)
    - [主な機能](#主な機能)
    - [セットアップ](#セットアップ)
        - [前提条件](#前提条件)
        - [クローン](#クローン)
        - [ビルド](#ビルド)
            - [ドキュメントツールのセットアップ（任意）](#ドキュメントツールのセットアップ任意)
    - [使用方法](#使用方法)
        - [Worker サービスの起動](#worker-サービスの起動)
        - [Web UI の起動](#web-ui-の起動)
    - [ドキュメント](#ドキュメント)
    - [サードパーティライセンス](#サードパーティライセンス)
    - [セキュリティ](#セキュリティ)
    - [謝辞](#謝辞)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 主な機能

- **マルチトポロジ対応** - Hub-Spoke（親子）および Peer-to-Peer トポロジをサポート
- **競合解決戦略** - SourceWins / LastWriteWins / ManualResolution の3つの戦略から選択
- **カラムレベル制御** - 同期対象カラムの Include / Exclude フィルタリング
- **マルチDBMS** - SQL Server、PostgreSQL、MySQL の Pleasanter インスタンスおよび構成データベースに対応
- **同期キー** - 任意のカラムをキーとしたレコードマッチング
- **削除同期** - ソフトデリート追跡による削除レコードの同期
- **監査ログ** - すべての同期操作を詳細に記録
- **Web UI** - Blazor Server ベースの管理画面でインスタンス・定義・ログを管理
- **バックグラウンドサービス** - .NET Worker Service による継続的なポーリング同期

## セットアップ

### 前提条件

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) 以上
- [Node.js](https://nodejs.org/) （ドキュメントのlint・フォーマット用、推奨）
- [Git](https://git-scm.com/)

### クローン

```bash
git clone https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ReplicaSync.git
cd VehicleVision.Pleasanter.ReplicaSync
```

### ビルド

```bash
dotnet restore
dotnet build
```

#### ドキュメントツールのセットアップ（任意）

```bash
npm install
```

## 使用方法

### Worker サービスの起動

バックグラウンドで同期処理を実行する Worker サービスを起動します。

```bash
dotnet run --project src/ReplicaSync.Worker
```

`appsettings.json` で構成データベースの接続先を設定できます。

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "<構成データベースの接続文字列>"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

`ConfigDatabaseType` には `SqlServer`、`PostgreSql` を指定できます。

### Web UI の起動

同期インスタンス・定義・ログを管理するための Web UI を起動します。

```bash
dotnet run --project src/ReplicaSync.Web
```

ブラウザで表示されるダッシュボードから、以下の操作が可能です：

- 同期インスタンスの登録・編集・削除
- 同期定義の作成・有効化・無効化
- 同期ログの確認

詳細は [Wiki](docs/wiki/Home.md) を参照してください。

## ドキュメント

- [Wiki](docs/wiki/Home.md) - 機能説明、アーキテクチャ、設定ガイド
- [コントリビューションガイド](CONTRIBUTING.md) - 開発参加方法

## サードパーティライセンス

このプロジェクトは以下のサードパーティライブラリを使用しています：

<!-- markdownlint-disable MD013 -->

| ライブラリ                              | ライセンス                  | 用途                            |
| --------------------------------------- | --------------------------- | ------------------------------- |
| Microsoft.EntityFrameworkCore           | MIT                         | ORM / 構成データベースアクセス  |
| Microsoft.EntityFrameworkCore.SqlServer | MIT                         | SQL Server プロバイダー         |
| Microsoft.Extensions.Hosting            | MIT                         | Worker サービスホスティング     |
| Microsoft.Extensions.Logging            | MIT                         | ログ抽象化                      |
| Microsoft.Data.SqlClient                | MIT                         | SQL Server ADO.NET ドライバー   |
| Npgsql                                  | PostgreSQL                  | PostgreSQL ADO.NET ドライバー   |
| Npgsql.EntityFrameworkCore.PostgreSQL   | PostgreSQL                  | PostgreSQL EF Core プロバイダー |
| MySqlConnector                          | MIT                         | MySQL ADO.NET ドライバー        |
| MySql.EntityFrameworkCore               | GPL-2.0 with FOSS exception | MySQL EF Core プロバイダー      |

<!-- markdownlint-enable MD013 -->

ライセンスファイルの全文は [LICENSES](./LICENSES/) フォルダを参照してください。

## セキュリティ

セキュリティ上の脆弱性を発見された場合は、[セキュリティポリシー](.github/SECURITY.md)をご確認の上、ご報告ください。

## 謝辞

セキュリティ脆弱性の報告やプロジェクトへの貢献をしてくださった方々に感謝いたします。

<!-- 貢献者・報告者はこちらに追記 -->
