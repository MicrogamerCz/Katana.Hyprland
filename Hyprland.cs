using System.Text.Json;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;

namespace Katana.Libraries;

public class Hyprland : INotifyPropertyChanged
{
    object _lock = new();

    JsonObject activeWindow, activeWorkspace;
    JsonArray workspaces;
    readonly string[] ignoreMessages = ["openlayer>>", "closelayer>>", "activewindowv2>>", "createworkspacev2>>", "activelayout>>"];
    
    public event PropertyChangedEventHandler? PropertyChanged;

    public JsonObject ActiveWindow
    {
        get => activeWindow;
        private set => _ = SetField(ref activeWindow, value);
    }
    public JsonObject ActiveWorkspace
    {
        get => activeWorkspace;
        private set => _ = SetField(ref activeWorkspace, value);
    }
    public JsonArray Workspaces
    {
        get => workspaces;
        private set => _ = SetField(ref workspaces, value);
    }

    static string SocketPath
    {
        get
        {
            string runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR")!;
            string hyprlandSignature = Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE")!;
            return Path.Combine(runtimeDir, "hypr", hyprlandSignature, ".socket.sock");
        }
    }
    static string Socket2Path
    {
        get
        {
            string runtimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR")!;
            string hyprlandSignature = Environment.GetEnvironmentVariable("HYPRLAND_INSTANCE_SIGNATURE")!;
            return Path.Combine(runtimeDir, "hypr", hyprlandSignature, ".socket2.sock");
        }
    }
    
    public void MoveToWorkspace(string workspace)
    {
        if (int.TryParse(workspace, out _)) Send($"workspace>>{workspace}");
        else if (workspace == "Down") Send("dispatch workspace +1");
        else if (workspace == "Up") Send("dispatch workspace -1");
    }

    public void Listen()
    {
        IterateListeners(); // For better experience

        using Socket clientSocket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        clientSocket.Connect(new UnixDomainSocketEndPoint(Socket2Path));

        while (true)
        {
            byte[] buffer = new byte[256];
            _ = clientSocket.Receive(buffer);

            string message = Encoding.UTF8.GetString(buffer);
            if (ignoreMessages.Any(message.StartsWith)) continue;

            IterateListeners();
        }
    }
    void IterateListeners()
    {
        lock (_lock)
        {
            ActiveWindow = (JsonObject)JsonObject.Parse(Get("activewindow"));
            ActiveWorkspace = (JsonObject)JsonObject.Parse(Get("activeworkspace"));
            Workspaces = (JsonArray)JsonArray.Parse(Get("workspaces"));
        }
    }
    public static void Send(string command)
    {
        using (Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
        {
            socket.Connect(new UnixDomainSocketEndPoint(SocketPath));
            _ = socket.Send(Encoding.UTF8.GetBytes(command));
        }
    }
    public static string Get(string property, int bufferSize = 1024)
    {
        string workspaceData;

        using (Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified))
        {
            socket.Connect(new UnixDomainSocketEndPoint(SocketPath));

            _ = socket.Send(Encoding.UTF8.GetBytes("-j/" + property));

            byte[] buffer = new byte[bufferSize];
            _ = socket.Receive(buffer);
            workspaceData = Encoding.UTF8.GetString(buffer);
        }

        try
        {
            using (JsonDocument.Parse(workspaceData)) {}
            return workspaceData;
        }
        catch (JsonException) { return Get(property, bufferSize * 2); }
    }
    bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
