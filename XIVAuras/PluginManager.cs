﻿using System;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using XIVAuras.Auras;
using XIVAuras.Config;
using XIVAuras.Helpers;
using XIVAuras.Windows;

namespace XIVAuras
{
    public class PluginManager : IPluginDisposable
    {
        private ClientState ClientState { get; init; }

        private DalamudPluginInterface PluginInterface { get; init; }

        private CommandManager CommandManager { get; init; }

        private WindowSystem WindowSystem { get; init; }

        private ConfigWindow ConfigRoot { get; init; }

        private XIVAurasConfig Config { get; init; }

        private readonly Vector2 _configSize = new Vector2(550, 600);

        private readonly ImGuiWindowFlags _mainWindowFlags = 
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.AlwaysAutoResize |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoSavedSettings;

        public PluginManager(
            ClientState clientState,
            CommandManager commandManager,
            DalamudPluginInterface pluginInterface,
            XIVAurasConfig config)
        {
            this.ClientState = clientState;
            this.CommandManager = commandManager;
            this.PluginInterface = pluginInterface;
            this.Config = config;

            this.ConfigRoot = new ConfigWindow("ConfigRoot", ImGui.GetMainViewport().Size / 2, _configSize);
            this.WindowSystem = new WindowSystem("XIVAuras");
            this.WindowSystem.AddWindow(this.ConfigRoot);

            this.CommandManager.AddHandler(
                "/xa",
                new CommandInfo(PluginCommand)
                {
                    HelpMessage = "Opens the XIVAuras configuration window.",
                    ShowInHelp = true
                }
            );

            this.ClientState.Logout += OnLogout;
            this.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
            this.PluginInterface.UiBuilder.Draw += Draw;
        }

        private void Draw()
        {
            if (!CharacterState.ShouldBeVisible())
            {
                return;
            }

            this.WindowSystem.Draw();

            Vector2 viewPortSize = ImGui.GetMainViewport().Size;
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.SetNextWindowSize(viewPortSize);
            if (ImGui.Begin("XIVAuras_Root", this._mainWindowFlags))
            {
                GroupOverrides groupOverrides = new GroupOverrides(viewPortSize / 2);
                foreach (AuraListItem aura in this.Config.AuraList.Auras)
                {
                    aura.Draw(groupOverrides.Merge(this.Config.GroupConfig));
                    groupOverrides.Unmerge(this.Config.GroupConfig);
                }
            }

            ImGui.End();
        }

        public void Edit(IConfigurable config)
        {
            this.ConfigRoot.PushConfig(config);
        }

        public bool IsConfigOpen() => this.ConfigRoot.IsOpen;

        private void OpenConfigUi()
        {
            if (!this.ConfigRoot.IsOpen)
            {
                this.ConfigRoot.PushConfig(this.Config);
            }
        }

        private void OnLogout(object? sender, EventArgs? args)
        {
            ConfigHelpers.SaveConfig();
        }
        
        private void PluginCommand(string command, string arguments)
        {
            if (this.ConfigRoot.IsOpen)
            {
                this.ConfigRoot.IsOpen = false;
            }
            else
            {
                this.ConfigRoot.PushConfig(this.Config);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Don't modify order
                this.PluginInterface.UiBuilder.Draw -= Draw;
                this.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
                this.ClientState.Logout -= OnLogout;
                this.CommandManager.RemoveHandler("/xa");
                this.WindowSystem.RemoveAllWindows();
            }
        }
    }
}
