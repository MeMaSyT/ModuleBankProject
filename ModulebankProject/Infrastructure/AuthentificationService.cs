namespace ModulebankProject.Infrastructure
{
    public class AuthentificationService
    {
        public static bool IsAuthentificated(Guid ownerId)
        {
            if(ownerId != Guid.Empty) return true;
            return false;
        }
    }
}
