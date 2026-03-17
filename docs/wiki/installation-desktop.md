# デスクトップアプリ インストールガイド

VehicleVision.Pleasanter.ReplicaSync のデスクトップ版クライアントアプリケーションのインストール手順です。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [概要](#概要)
- [前提条件](#前提条件)
    - [共通](#共通)
    - [Windows](#windows)
    - [Linux](#linux)
    - [macOS](#macos)
- [ソースからのビルド・実行](#ソースからのビルド実行)
- [設定](#設定)
    - [データベース接続](#データベース接続)
    - [セキュリティ](#セキュリティ)
- [Web 版との違い](#web-版との違い)
- [トラブルシューティング](#トラブルシューティング)
    - [WebView2 が見つからない（Windows）](#webview2-が見つからないwindows)
    - [libwebkit2gtk が見つからない（Linux）](#libwebkit2gtk-が見つからないlinux)
    - [ポート競合](#ポート競合)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 概要

デスクトップ版は、Web 版と同じ管理 UI をネイティブウィンドウで提供します。
内蔵 Web サーバ（Kestrel）が自動的に起動するため、別途サーバを構築する必要はありません。

| 項目           | 内容                                                                 |
| -------------- | -------------------------------------------------------------------- |
| ランタイム     | .NET 10                                                              |
| WebView エンジン | Windows: WebView2 / Linux: WebKitGTK / macOS: WKWebView            |
| 認証方式       | Cookie 認証（ローカルブラウザ内で完結）                              |

## 前提条件

### 共通

- .NET 10 ランタイムがインストールされていること
- 対応データベース（SQL Server / PostgreSQL / MySQL）に接続できること

### Windows

WebView2 ランタイムが必要です（Windows 10 バージョン 1803 以降では標準搭載）。

未インストールの場合は [Microsoft 公式サイト](https://developer.microsoft.com/en-us/microsoft-edge/webview2/)からダウンロードしてください。

### Linux

WebKitGTK パッケージが必要です。

```bash
# Debian / Ubuntu
sudo apt install libwebkit2gtk-4.1-0

# Fedora / AlmaLinux
sudo dnf install webkit2gtk4.1
```

### macOS

WKWebView は macOS に標準搭載されているため、追加インストールは不要です。

## ソースからのビルド・実行

```bash
# リポジトリのクローン
git clone https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ReplicaSync.git
cd VehicleVision.Pleasanter.ReplicaSync

# ビルド
dotnet build src/VehicleVision.Pleasanter.ReplicaSync.Desktop

# 実行
dotnet run --project src/VehicleVision.Pleasanter.ReplicaSync.Desktop
```

## 設定

設定ファイル `appsettings.json` は Desktop プロジェクトのルートにあります。

### データベース接続

```json
{
  "ConnectionStrings": {
    "ConfigDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=ReplicaSync;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "ConfigDatabaseType": "SqlServer"
}
```

`ConfigDatabaseType` には `SqlServer`、`PostgreSql`、`MySql` のいずれかを指定します。

### セキュリティ

```json
{
  "Security": {
    "InitialAdminUsername": "administrator",
    "InitialAdminPassword": "vehiclevision",
    "AccountLockout": {
      "Enabled": true,
      "MaxFailedAttempts": 5,
      "LockoutDurationMinutes": 15
    }
  }
}
```

初回起動時に上記の初期管理者ユーザーが自動作成されます。
ログイン後、パスワード変更が求められます。

## Web 版との違い

| 機能               | Web 版                | デスクトップ版              |
| ------------------ | --------------------- | --------------------------- |
| Web サーバ         | 外部公開可能な Kestrel / IIS | localhost 限定の内蔵 Kestrel |
| API キー認証       | あり（Worker 連携用） | なし                        |
| IP ホワイトリスト  | あり                  | なし                        |
| Web API エンドポイント | あり               | なし                        |
| クロスプラットフォーム | ブラウザ依存       | Windows / Linux / macOS     |

## トラブルシューティング

### WebView2 が見つからない（Windows）

WebView2 ランタイムをインストールしてください。

### libwebkit2gtk が見つからない（Linux）

WebKitGTK パッケージをインストールしてください（[前提条件](#linux)参照）。

### ポート競合

デスクトップ版は起動時に空きポートを自動的に選択するため、通常ポート競合は発生しません。
