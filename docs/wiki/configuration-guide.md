# 設定ガイド

このドキュメントでは、VehicleVision.Pleasanter.ReplicaSync の設定方法について説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [構成データベース](#構成データベース)
- [同期インスタンス（SyncInstance）](#同期インスタンスsyncinstance)
    - [設定例](#設定例)
- [同期定義（SyncDefinition）](#同期定義syncdefinition)
- [ターゲットマッピング（SyncTargetMapping）](#ターゲットマッピングsynctargetmapping)
- [カラム指定](#カラム指定)
    - [基本カラム](#基本カラム)
    - [拡張カラム](#拡張カラム)
    - [指定例](#指定例)
- [ロギング設定](#ロギング設定)
    - [設定ファイル](#設定ファイル)
    - [出力先（ターゲット）](#出力先ターゲット)
    - [ログファイル](#ログファイル)
    - [ログレベルの変更](#ログレベルの変更)
- [セキュリティ設定](#セキュリティ設定)
    - [ユーザー認証（Cookie 認証）](#ユーザー認証cookie-認証)
    - [API キー認証](#api-キー認証)
    - [IP アドレス制限](#ip-アドレス制限)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 構成データベース

同期設定を保存する構成データベースの接続先は `appsettings.json` で設定します。

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "<接続文字列>"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

| 設定項目             | 説明                         | 既定値      |
| -------------------- | ---------------------------- | ----------- |
| `ConfigDatabase`     | 構成データベースの接続文字列 | -           |
| `ConfigDatabaseType` | 構成データベースの種類       | `SqlServer` |

`ConfigDatabaseType` に指定可能な値：

| 値           | 説明       |
| ------------ | ---------- |
| `SqlServer`  | SQL Server |
| `PostgreSql` | PostgreSQL |
| `MySql`      | MySQL      |

## 同期インスタンス（SyncInstance）

同期対象となる Pleasanter インスタンスの接続情報を登録します。

| プロパティ         | 型         | 説明                                                       |
| ------------------ | ---------- | ---------------------------------------------------------- |
| `InstanceId`       | `string`   | インスタンスの一意識別子（例: `headquarters`）             |
| `DisplayName`      | `string`   | 表示名                                                     |
| `DbmsType`         | `DbmsType` | データベースの種類（`SqlServer` / `PostgreSql` / `MySql`） |
| `ConnectionString` | `string`   | Pleasanter データベースへの接続文字列                      |

### 設定例

```text
InstanceId:       headquarters
DisplayName:      本社
DbmsType:         SqlServer
ConnectionString: Server=localhost;Database=Implem.Pleasanter;Trusted_Connection=True;
```

## 同期定義（SyncDefinition）

データ同期のルールを定義します。

| プロパティ                  | 型                           | 説明                                             |
| --------------------------- | ---------------------------- | ------------------------------------------------ |
| `SyncId`                    | `string`                     | 同期定義の一意識別子                             |
| `Description`               | `string`                     | 説明                                             |
| `Topology`                  | `TopologyType`               | `HubSpoke` または `PeerToPeer`                   |
| `ConflictResolution`        | `ConflictResolutionStrategy` | 競合解決戦略                                     |
| `ChangeDetectionMethod`     | `ChangeDetectionMethod`      | 変更検出方法（現在は `Polling` のみ）            |
| `PollingIntervalSeconds`    | `int`                        | ポーリング間隔（1～3600秒）                      |
| `SyncUserId`                | `int`                        | 同期に使用する Pleasanter ユーザーID             |
| `SyncUserName`              | `string`                     | 同期ユーザーの表示名                             |
| `SourceInstanceId`          | `int`                        | ソースインスタンスの ID                          |
| `SourceSiteId`              | `long`                       | ソース Pleasanter サイト ID                      |
| `IsEnabled`                 | `bool`                       | 同期定義の有効/無効                              |
| `SyncKeyColumns`            | `string`                     | 同期キーカラム（カンマ区切り）                   |
| `IncludeColumns`            | `string`                     | 同期対象カラム（カンマ区切り、省略時は全カラム） |
| `ExcludeColumns`            | `string`                     | 同期除外カラム（カンマ区切り）                   |
| `RecordFilterInclude`       | `string?`                    | レコードフィルタ（含む条件、JSON）               |
| `RecordFilterExclude`       | `string?`                    | レコードフィルタ（除外条件、JSON）               |
| `AttachmentsEnabled`        | `bool`                       | 添付ファイル同期の有効/無効                      |
| `AttachmentsStorageType`    | `string`                     | 添付ファイルの保存方式（`Rds` のみ対応）         |
| `VersionHistoryEnabled`     | `bool`                       | バージョン履歴の有効/無効（既定: `true`）        |
| `VersionHistoryMaxVersions` | `int?`                       | 最大保持版数（`null` = 無制限、既定: 20）        |
| `VersionHistoryMaxDays`     | `int?`                       | 最大保持日数（`null` = 無制限、既定: 180）       |

## ターゲットマッピング（SyncTargetMapping）

同期定義に対するターゲットインスタンスのマッピングを設定します。

| プロパティ                     | 型        | 説明                                           |
| ------------------------------ | --------- | ---------------------------------------------- |
| `TargetInstanceId`             | `int`     | ターゲットインスタンスの ID                    |
| `TargetSiteId`                 | `long`    | ターゲット Pleasanter サイト ID                |
| `SourceToTargetEnabled`        | `bool`    | ソースからターゲットへの同期を有効化           |
| `TargetToSourceEnabled`        | `bool`    | ターゲットからソースへの同期を有効化           |
| `TargetToSourceExcludeColumns` | `string`  | ターゲットからソースへの同期で除外するカラム   |
| `TargetExcludeColumns`         | `string`  | ターゲット側で除外するカラム                   |
| `RecordFilterIncludeOverride`  | `string?` | レコードフィルタ（含む条件）のターゲット上書き |
| `RecordFilterExcludeOverride`  | `string?` | レコードフィルタ（除外条件）のターゲット上書き |

## カラム指定

Pleasanter のデータカラムは以下の形式で指定します。

### 基本カラム

- `Title` - タイトル
- `Body` - 本文

### 拡張カラム

- `ClassA` ～ `ClassZ` - 分類項目
- `NumA` ～ `NumZ` - 数値項目
- `DateA` ～ `DateZ` - 日付項目
- `DescriptionA` ～ `DescriptionZ` - 説明項目

### 指定例

```text
SyncKeyColumns:  ClassA
IncludeColumns:  Title,ClassA,ClassB,NumA
ExcludeColumns:  DescriptionZ
```

上記の場合、`ClassA` をキーとしてレコードを照合し、`Title`、`ClassA`、`ClassB`、`NumA` のみを同期対象とします。
`ExcludeColumns` は `IncludeColumns` が未指定の場合に全カラムから除外するカラムを指定する際に使用します。

## ロギング設定

本プロジェクトでは [NLog](https://nlog-project.org/) を使用してロギングを行います。
コンソール出力、デバッグコンソール出力、ファイル出力はすべて NLog 経由で統一されています。

### 設定ファイル

各プロジェクトのルートに `nlog.config` を配置しています。

- `src/ReplicaSync.Worker/nlog.config` — Worker サービス用
- `src/ReplicaSync.Web/nlog.config` — Web UI 用

### 出力先（ターゲット）

| ターゲット  | 説明                               |
| ----------- | ---------------------------------- |
| `console`   | コンソール出力（ColoredConsole）   |
| `debugger`  | デバッグコンソール出力（Debugger） |
| `file`      | 日付ローテーションのファイル出力   |
| `errorFile` | Error 以上のログ専用ファイル出力   |

### ログファイル

ログファイルは実行ディレクトリの `logs/` フォルダに出力されます。

```text
logs/
├── replicasync-worker-2026-03-17.log        # Worker 通常ログ
├── replicasync-worker-error-2026-03-17.log  # Worker エラーログ
├── replicasync-web-2026-03-17.log           # Web 通常ログ
├── replicasync-web-error-2026-03-17.log     # Web エラーログ
└── nlog-internal.log                        # NLog 内部ログ
```

### ログレベルの変更

`nlog.config` の `<rules>` セクションで `minlevel` を変更することでログレベルを調整できます。

```xml
<!-- 例: Trace レベルまで出力 -->
<logger name="*" minlevel="Trace" writeTo="console,debugger,file" />
```

## セキュリティ設定

管理 Web アプリケーションのセキュリティは `appsettings.json` の `Security` セクションで設定します。

```json
{
    "Security": {
        "InitialAdminUsername": "administrator",
        "InitialAdminPassword": "vehiclevision",
        "ApiKeys": [
            {
                "Name": "worker-service",
                "Key": "<APIキー>"
            }
        ],
        "IpWhitelist": {
            "Enabled": false,
            "AllowedAddresses": ["127.0.0.1", "::1"]
        }
    }
}
```

### ユーザー認証（Cookie 認証）

Web UI へのアクセスはユーザー ID / パスワードによる Cookie 認証で保護されています。
ユーザー情報は構成データベースの `AppUsers` テーブルで管理されます。

#### 初期ユーザー

アプリケーション初回起動時に、ユーザーが存在しない場合は以下の設定で初期管理者が自動作成されます。

| 設定項目               | 既定値          | 説明                    |
| ---------------------- | --------------- | ----------------------- |
| `InitialAdminUsername` | `administrator` | 初期管理者のユーザー ID |
| `InitialAdminPassword` | `vehiclevision` | 初期管理者のパスワード  |

初期ユーザーは `MustChangePassword` フラグが有効な状態で作成されるため、**初回ログイン時にパスワード変更が強制**されます。

> **注意**: 本番環境では `InitialAdminPassword` を安全な値に変更した上でデプロイしてください。初期ユーザー作成後は設定値から削除しても問題ありません。

#### パスワード保存方式

| パラメータ   | 値                                                              |
| ------------ | --------------------------------------------------------------- |
| アルゴリズム | PBKDF2 + SHA-256                                                |
| 反復回数     | 100,000                                                         |
| ソルト長     | 16 バイト（ランダム生成）                                       |
| ハッシュ長   | 32 バイト                                                       |
| 比較方法     | `CryptographicOperations.FixedTimeEquals`（timing attack 対策） |

- 認証済みセッションは 8 時間有効（スライディング有効期限）
- Cookie は `HttpOnly` / `SameSite=Strict` で保護されています

### API キー認証

WebAPI 経由でのアクセスには `X-Api-Key` HTTP ヘッダーによる API キー認証を使用します。

| 設定項目 | 説明                               |
| -------- | ---------------------------------- |
| `Name`   | API キーの識別名（ログ出力に使用） |
| `Key`    | API キーの値                       |

リクエスト例：

```bash
curl -H "X-Api-Key: <APIキー>" https://localhost:5001/api/sync-definitions
```

### IP アドレス制限

`IpWhitelist` セクションでクライアント IP アドレスによるアクセス制限を設定します。

| 設定項目           | 型      | 説明                                         |
| ------------------ | ------- | -------------------------------------------- |
| `Enabled`          | `bool`  | IP 制限を有効にするか（`false` で無効）      |
| `AllowedAddresses` | `array` | 許可する IP アドレスまたは CIDR 表記のリスト |

CIDR 表記に対応しているため、サブネット単位での制限が可能です。

```json
{
    "AllowedAddresses": ["127.0.0.1", "::1", "192.168.1.0/24", "10.0.0.0/8"]
}
```

> **注意**: `Enabled` が `false` の場合、IP 制限は適用されません。本番環境では `true` に設定し、必要な IP アドレスのみを許可することを推奨します。
