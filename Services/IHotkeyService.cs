namespace YUK1.Services;

public interface IHotkeyService
{
    public bool IsRegistered { get; }
    public bool Register(string hotkeyExpression, Action callback);
    public void Unregister();
}
