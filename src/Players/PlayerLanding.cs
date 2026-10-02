namespace PeakAdminToolkit.Players
{
    internal interface IPlayerLanding
    {
        bool TryNear(object anchor, out object position);
        bool TryInFront(object anchor, out object position);
    }
}
