// =============================================
// AKKADIAN GENERATED C# MODELS
// Generated: 12/14/2025 11:19:22 PM
// =============================================
using System;

namespace NajafCemeteryMaster.Domain
{
    /// <summary>
    /// Optimized Data Structure for hub_deceased_record
    /// Memory Layout: Sequential (Stack/Array optimized)
    /// </summary>
    public readonly record struct HubDeceasedRecord
    (
        string TribalColorId,
        string CivilId,
        DateTime LoadDate,
        string RecordSource
    );

    /// <summary>
    /// Optimized Data Structure for sat_burial_info
    /// Memory Layout: Sequential (Stack/Array optimized)
    /// </summary>
    public readonly record struct SatBurialInfo
    (
        string TribalColorId,
        string DeathDate,
        string BurialDate,
        string DeathCertificateImg,
        DateTime LoadDate,
        string RecordSource,
        DateTime ValidFrom,
        DateTime? ValidTo,
        bool IsCurrent
    );

}
