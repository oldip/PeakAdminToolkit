namespace PeakAdminToolkit.UI
{
    internal sealed class Toast
    {
        private string message;
        private double expires;
        public void Show(string text, double now, double duration)
        {
            message = text;
            expires = now + duration;
        }
        public string Current(double now) { return now < expires ? message : null; }
        public void Clear() { message = null; }
    }
}
