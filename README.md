A feature-incompelete C# library for [Hyprland](https://github.com/hyprwm/hyprland)

**Dependencies**: Hyprland 

- - -
# API

```csharp
JsonObject ActiveWindow { get; }
JsonObject ActiveWorkspace { get; }
JsonArray Workspaces { get; }

// Numeral string for a specific workspace
// 'Down' for +1, 'Up' for -1 
void MoveToWorkspace(string workspace);
    
void Send(string command); // Sends a hyprctl command
string Get(string property); // Sends a hyprctl command returning JSON string value
```
