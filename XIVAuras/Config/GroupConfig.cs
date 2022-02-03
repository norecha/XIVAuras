using System.Numerics;
using ImGuiNET;

namespace XIVAuras.Config
{
    public class GroupConfig : IConfigPage
    {
        public string Name => "Group";

        public Vector2 Position = new Vector2(0, 0);
        public Vector2 IconSize = new Vector2(0, 0);

        public IConfigPage GetDefault() => new GroupConfig();

        public void DrawConfig(Vector2 size, float padX, float padY)
        {
            if (ImGui.BeginChild("##GroupConfig", new Vector2(size.X, size.Y), true))
            {
                ImGui.DragFloat2("Group Position", ref this.Position);
                ImGui.DragFloat2("Icon Size", ref this.IconSize, 1, 0, float.MaxValue);
                ImGui.EndChild();
            }
        }
    }
}
