namespace Linker.Ui.Components
{
    public static class ToastService
    {
        public static event System.Action<string> Requested;

        public static void Show(string message)
        {
            Requested?.Invoke(message);
        }
    }
}
