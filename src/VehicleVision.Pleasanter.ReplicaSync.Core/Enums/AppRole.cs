namespace VehicleVision.Pleasanter.ReplicaSync.Core.Enums;

/// <summary>
/// アプリケーションの権限ロール。
/// </summary>
public enum AppRole
{
    /// <summary>通常ユーザー（同期状況の閲覧のみ）。</summary>
    User,

    /// <summary>管理者（ユーザー管理・設定変更が可能）。</summary>
    Administrator,
}
