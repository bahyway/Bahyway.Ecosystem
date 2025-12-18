// =============================================
// AKKADIAN GENERATED C# MODELS
// Generated: 12/14/2025 11:08:45 PM
// =============================================
using System;

namespace WPDWayOptimized.Domain
{
    /// <summary>
    /// Optimized Data Structure for hub_pipe_segment
    /// Memory Layout: Sequential (Stack/Array optimized)
    /// </summary>
    public readonly record struct HubPipeSegment
    (
        string ColorId,
        Guid SegmentUuid,
        string Material,
        int DiameterMm,
        DateTime LoadDate,
        string RecordSource
    );

    /// <summary>
    /// Optimized Data Structure for link_pipe_connection
    /// Memory Layout: Sequential (Stack/Array optimized)
    /// </summary>
    public readonly record struct LinkPipeConnection
    (
        string UpstreamColorId,
        string DownstreamColorId,
        string ConnectionType,
        DateTime LoadDate,
        string RecordSource
    );

    /// <summary>
    /// Optimized Data Structure for sat_defect_metrics
    /// Memory Layout: Sequential (Stack/Array optimized)
    /// </summary>
    public readonly record struct SatDefectMetrics
    (
        string ColorId,
        decimal WallThickness,
        decimal PressurePsi,
        decimal SoilAcidityPh,
        DateTime LoadDate,
        string RecordSource,
        DateTime ValidFrom,
        DateTime? ValidTo,
        bool IsCurrent
    );

}
