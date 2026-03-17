# Web API リファレンス

このドキュメントでは、VehicleVision.Pleasanter.ReplicaSync が提供する Web API エンドポイントについて説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [認証](#認証)
- [共通仕様](#共通仕様)
- [ユーザー管理 API](#ユーザー管理-api)
    - [GET /api/users](#get-apiusers)
    - [GET /api/users/{id}](#get-apiusersid)
    - [POST /api/users](#post-apiusers)
    - [PUT /api/users/{id}](#put-apiusersid)
    - [DELETE /api/users/{id}](#delete-apiusersid)
- [同期インスタンス API](#同期インスタンス-api)
    - [GET /api/sync-instances](#get-apisync-instances)
    - [GET /api/sync-instances/{id}](#get-apisync-instancesid)
    - [POST /api/sync-instances](#post-apisync-instances)
    - [PUT /api/sync-instances/{id}](#put-apisync-instancesid)
    - [DELETE /api/sync-instances/{id}](#delete-apisync-instancesid)
- [同期定義 API](#同期定義-api)
    - [GET /api/sync-definitions](#get-apisync-definitions)
    - [GET /api/sync-definitions/{id}](#get-apisync-definitionsid)
    - [POST /api/sync-definitions](#post-apisync-definitions)
    - [PUT /api/sync-definitions/{id}](#put-apisync-definitionsid)
    - [DELETE /api/sync-definitions/{id}](#delete-apisync-definitionsid)
- [同期ログ API](#同期ログ-api)
    - [GET /api/sync-logs](#get-apisync-logs)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 認証

すべての API エンドポイントは `X-Api-Key` ヘッダーによる API キー認証が必要です。

```bash
curl -H "X-Api-Key: <APIキー>" https://localhost:5001/api/users
```

API キーの設定方法は[設定ガイド](configuration-guide.md#api-キー認証)を参照してください。

## 共通仕様

| 項目             | 仕様                                     |
| ---------------- | ---------------------------------------- |
| Content-Type     | `application/json`                       |
| 認証             | `X-Api-Key` ヘッダー                     |
| エラーレスポンス | `{ "message": "エラーメッセージ" }` 形式 |

---

## ユーザー管理 API

### GET /api/users

すべてのユーザーを取得します。

**レスポンス例:**

```json
[
    {
        "id": 1,
        "username": "administrator",
        "role": "Administrator",
        "mustChangePassword": false,
        "createdAt": "2026-03-17T00:00:00",
        "updatedAt": "2026-03-17T00:00:00"
    }
]
```

### GET /api/users/{id}

指定した ID のユーザーを取得します。

| ステータス | 説明                     |
| ---------- | ------------------------ |
| 200        | ユーザー情報を返却       |
| 404        | 指定 ID のユーザーが不在 |

### POST /api/users

新しいユーザーを作成します。

**リクエストボディ:**

```json
{
    "username": "newuser",
    "password": "securepassword",
    "role": "User",
    "mustChangePassword": true
}
```

| フィールド           | 型       | 必須 | 説明                                   |
| -------------------- | -------- | ---- | -------------------------------------- |
| `username`           | `string` | ○    | ユーザー名（最大100文字）              |
| `password`           | `string` | ○    | パスワード（最小8文字）                |
| `role`               | `string` | -    | `User` または `Administrator`          |
| `mustChangePassword` | `bool`   | -    | パスワード変更必須フラグ（既定: true） |

| ステータス | 説明                           |
| ---------- | ------------------------------ |
| 201        | 作成されたユーザー情報を返却   |
| 409        | ユーザー名が既に使用されている |

### PUT /api/users/{id}

指定した ID のユーザーを更新します。

**リクエストボディ:**

```json
{
    "username": "updateduser",
    "role": "Administrator",
    "mustChangePassword": false,
    "password": "newpassword"
}
```

| フィールド           | 型       | 必須 | 説明                                           |
| -------------------- | -------- | ---- | ---------------------------------------------- |
| `username`           | `string` | ○    | ユーザー名                                     |
| `role`               | `string` | -    | `User` または `Administrator`                  |
| `mustChangePassword` | `bool`   | -    | パスワード変更必須フラグ                       |
| `password`           | `string` | -    | 新しいパスワード（省略時はパスワード変更なし） |

| ステータス | 説明                           |
| ---------- | ------------------------------ |
| 200        | 更新されたユーザー情報を返却   |
| 404        | 指定 ID のユーザーが不在       |
| 409        | ユーザー名が既に使用されている |

### DELETE /api/users/{id}

指定した ID のユーザーを削除します。

| ステータス | 説明                     |
| ---------- | ------------------------ |
| 204        | 削除成功                 |
| 404        | 指定 ID のユーザーが不在 |

---

## 同期インスタンス API

### GET /api/sync-instances

すべての同期インスタンスを取得します。

**レスポンス例:**

```json
[
    {
        "id": 1,
        "instanceId": "headquarters",
        "displayName": "本社",
        "dbmsType": "SqlServer",
        "createdAt": "2026-03-17T00:00:00",
        "updatedAt": "2026-03-17T00:00:00"
    }
]
```

> **注意**: セキュリティ上、レスポンスには `connectionString` は含まれません。

### GET /api/sync-instances/{id}

指定した ID の同期インスタンスを取得します。

| ステータス | 説明                         |
| ---------- | ---------------------------- |
| 200        | インスタンス情報を返却       |
| 404        | 指定 ID のインスタンスが不在 |

### POST /api/sync-instances

新しい同期インスタンスを作成します。

**リクエストボディ:**

```json
{
    "instanceId": "branch-a",
    "displayName": "支店A",
    "dbmsType": "PostgreSql",
    "connectionString": "Host=192.168.1.10;Database=Implem.Pleasanter;Username=postgres;Password=xxx"
}
```

| フィールド         | 型       | 必須 | 説明                               |
| ------------------ | -------- | ---- | ---------------------------------- |
| `instanceId`       | `string` | ○    | インスタンス識別子（最大100文字）  |
| `displayName`      | `string` | ○    | 表示名（最大200文字）              |
| `dbmsType`         | `string` | -    | `SqlServer`、`PostgreSql`、`MySql` |
| `connectionString` | `string` | ○    | データベース接続文字列             |

| ステータス | 説明                             |
| ---------- | -------------------------------- |
| 201        | 作成されたインスタンス情報を返却 |

### PUT /api/sync-instances/{id}

指定した ID の同期インスタンスを更新します。リクエストボディは POST と同じ形式です。

| ステータス | 説明                             |
| ---------- | -------------------------------- |
| 200        | 更新されたインスタンス情報を返却 |
| 404        | 指定 ID のインスタンスが不在     |

### DELETE /api/sync-instances/{id}

指定した ID の同期インスタンスを削除します。

| ステータス | 説明                         |
| ---------- | ---------------------------- |
| 204        | 削除成功                     |
| 404        | 指定 ID のインスタンスが不在 |

---

## 同期定義 API

### GET /api/sync-definitions

すべての同期定義を取得します。

**レスポンス例:**

```json
[
    {
        "id": 1,
        "syncId": "master-employee",
        "description": "社員マスタ同期",
        "topology": "HubSpoke",
        "conflictResolution": "SourceWins",
        "changeDetectionMethod": "Polling",
        "pollingIntervalSeconds": 5,
        "syncUserId": 1,
        "syncUserName": "SyncService",
        "sourceInstanceId": 1,
        "sourceSiteId": 12345,
        "isEnabled": true,
        "syncKeyColumns": "ClassA",
        "includeColumns": "Title,ClassA,ClassB",
        "excludeColumns": "",
        "recordFilterInclude": null,
        "recordFilterExclude": null,
        "attachmentsEnabled": false,
        "attachmentsStorageType": "Rds",
        "versionHistoryEnabled": true,
        "versionHistoryMaxVersions": 20,
        "versionHistoryMaxDays": 180,
        "targetMappings": [
            {
                "id": 1,
                "targetInstanceId": 2,
                "targetSiteId": 67890,
                "sourceToTargetEnabled": true,
                "targetToSourceEnabled": false,
                "targetToSourceExcludeColumns": "",
                "targetExcludeColumns": "",
                "recordFilterIncludeOverride": null,
                "recordFilterExcludeOverride": null
            }
        ],
        "createdAt": "2026-03-17T00:00:00",
        "updatedAt": "2026-03-17T00:00:00"
    }
]
```

### GET /api/sync-definitions/{id}

指定した ID の同期定義を取得します。

| ステータス | 説明                     |
| ---------- | ------------------------ |
| 200        | 同期定義情報を返却       |
| 404        | 指定 ID の同期定義が不在 |

### POST /api/sync-definitions

新しい同期定義を作成します。

**リクエストボディ:**

```json
{
    "syncId": "master-employee",
    "description": "社員マスタ同期",
    "topology": "HubSpoke",
    "conflictResolution": "SourceWins",
    "changeDetectionMethod": "Polling",
    "pollingIntervalSeconds": 5,
    "syncUserId": 1,
    "syncUserName": "SyncService",
    "sourceInstanceId": 1,
    "sourceSiteId": 12345,
    "isEnabled": true,
    "syncKeyColumns": "ClassA",
    "includeColumns": "Title,ClassA,ClassB",
    "targetMappings": [
        {
            "targetInstanceId": 2,
            "targetSiteId": 67890,
            "sourceToTargetEnabled": true,
            "targetToSourceEnabled": false
        }
    ]
}
```

各フィールドの詳細は[設定ガイド](configuration-guide.md#同期定義syncdefinition)を参照してください。

| ステータス | 説明                         |
| ---------- | ---------------------------- |
| 201        | 作成された同期定義情報を返却 |

### PUT /api/sync-definitions/{id}

指定した ID の同期定義を更新します。リクエストボディは POST と同じ形式です。

| ステータス | 説明                         |
| ---------- | ---------------------------- |
| 200        | 更新された同期定義情報を返却 |
| 404        | 指定 ID の同期定義が不在     |

### DELETE /api/sync-definitions/{id}

指定した ID の同期定義を削除します。

| ステータス | 説明                     |
| ---------- | ------------------------ |
| 204        | 削除成功                 |
| 404        | 指定 ID の同期定義が不在 |

---

## 同期ログ API

### GET /api/sync-logs

直近の同期ログを取得します。

**クエリパラメータ:**

| パラメータ | 型    | 既定値 | 説明                    |
| ---------- | ----- | ------ | ----------------------- |
| `count`    | `int` | 100    | 取得する件数（1〜1000） |

**リクエスト例:**

```bash
curl -H "X-Api-Key: <APIキー>" "https://localhost:5001/api/sync-logs?count=50"
```

**レスポンス例:**

```json
[
    {
        "id": 1,
        "syncId": "master-employee",
        "sourceInstanceId": "headquarters",
        "targetInstanceId": "branch-a",
        "status": "Success",
        "recordsProcessed": 150,
        "recordsInserted": 5,
        "recordsUpdated": 3,
        "recordsDeleted": 0,
        "conflictsDetected": 0,
        "errorMessage": null,
        "startedAt": "2026-03-17T10:00:00",
        "completedAt": "2026-03-17T10:00:02"
    }
]
```

| ステータス | 説明                                  |
| ---------- | ------------------------------------- |
| 200        | ログ一覧を返却                        |
| 400        | `count` パラメータが範囲外（1〜1000） |
