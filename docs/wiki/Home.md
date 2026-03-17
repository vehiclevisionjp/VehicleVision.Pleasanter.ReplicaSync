# VehicleVision.Pleasanter.ReplicaSync Wiki

VehicleVision.Pleasanter.ReplicaSync のドキュメントへようこそ。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [利用者向けドキュメント](#利用者向けドキュメント)
    - [インストールガイド](#インストールガイド)
    - [設定・運用](#設定運用)
- [開発者向けドキュメント](#開発者向けドキュメント)
    - [内部設計](#内部設計)
    - [コントリビューション](#コントリビューション)
- [関連リンク](#関連リンク)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 利用者向けドキュメント

製品のインストール、設定、操作方法に関するドキュメントです。

### インストールガイド

- [NuGet パッケージ](installation-nuget.md) - NuGet を使用した最も簡単なセットアップ方法
- [デスクトップアプリ](installation-desktop.md) - Windows / Linux / macOS 対応のクライアントアプリ
- [Azure 環境](installation-azure.md) - Azure App Service / Azure SQL へのデプロイ手順
- [オンプレミス Windows](installation-windows.md) - Windows Server / IIS / Windows サービスでの構築手順
- [オンプレミス Linux](installation-linux.md) - Debian / Ubuntu / AlmaLinux での構築手順

### 設定・運用

- [設定ガイド](configuration-guide.md) - 同期インスタンス・定義の設定方法、ロギング、セキュリティ
- [Web UI 取扱説明書](web-manual.md) - 管理画面の操作方法
- [Web API リファレンス](web-api-reference.md) - REST API エンドポイントの仕様

---

## 開発者向けドキュメント

内部設計や開発への参加方法に関するドキュメントです。

### 内部設計

- [アーキテクチャ概要](architecture-overview.md) - システム構成、レイヤー構造、データフロー
- [データベース構造](database-schema.md) - 構成データベースのテーブル定義・ER 図
- [同期エンジン](sync-engine.md) - 同期処理の仕組みと競合解決戦略

### コントリビューション

<!-- markdownlint-disable MD013 -->

- [コントリビューションガイド](https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ReplicaSync/blob/main/CONTRIBUTING.md) - 開発環境のセットアップ、コントリビューションの手順

<!-- markdownlint-enable MD013 -->

## 関連リンク

- [README](https://github.com/vehiclevisionjp/VehicleVision.Pleasanter.ReplicaSync)
- [NuGet パッケージ - VehicleVision.Pleasanter.ReplicaSync.Web](https://www.nuget.org/packages/VehicleVision.Pleasanter.ReplicaSync.Web)
- [NuGet パッケージ - VehicleVision.Pleasanter.ReplicaSync.Worker](https://www.nuget.org/packages/VehicleVision.Pleasanter.ReplicaSync.Worker)
- [NuGet パッケージ - VehicleVision.Pleasanter.ReplicaSync.Desktop](https://www.nuget.org/packages/VehicleVision.Pleasanter.ReplicaSync.Desktop)
