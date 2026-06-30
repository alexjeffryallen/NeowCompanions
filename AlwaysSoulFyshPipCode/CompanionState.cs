namespace NeowCompanions.NeowCompanionsCode;

public enum CompanionKind
{
    None,
    Byrdpip,
    SoulFysh,
    Wriggler,
    CeremonialBeast,
    KinFollower,
    EyeWithTeeth,
    GremlinMerc,
    ThievingHopper,
    Aeonglass,
    LagavulinMatriarch,
    TheKin,
    WaterfallGiant,
    Vantom,
    KnowledgeDemon,
    TheInsatiable,
    Queen,
    TestSubject
}

public static class CompanionState
{
    public static CompanionKind SelectedCompanion { get; set; } = CompanionKind.None;
}
