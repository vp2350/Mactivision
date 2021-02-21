using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class Battery
{
    // Helpers
    private ConfigHandler Config;
    private SceneController Scene;
    private FileHandler FileHandle;

    // Config State
    private bool IsLoaded;

    public static readonly Battery Instance = new Battery(); 
    private Battery()
    { 
        Reset();
    }

    public void Reset()
    {
        Config = new ConfigHandler();
        IsLoaded = false;
    }

    public string GetGameName()
    {
        // Game name is part of the GameConfig interface so does not require casting to the specific game config. Useful to generating log files by name. Name is not the name of the game but that specific test of a game. TODO: Better naming.
        if (IsLoaded)
        {
            return Config.GetTestName(Scene.Current());
        }
        return null;
    }

    // Returns the GameConfig interface type. Specific games will have to cast the GameConfig to their respective Config class in order to child parameters. 
    public GameConfig GetCurrentConfig()
    {
        if (IsLoaded)
        {
            return Config.Get(Scene.Current());
        }
        return null;
    }

    // Load the BatteryConfig JSON file and deserialize it while maintaining type information. Currently uses TextAsset which is a Unity Resource type. This allows easier file reading but it may not be wise to clutter resource folder. 
    public void LoadBattery(string json)
    {
        Config.Load(json);
        Scene = new SceneController("Battery Start", "Battery End", Config.GameScenes());
        IsLoaded = true;
    }

    // Scenes are loaded by name
    public void LoadScene(string Scene)
    {
        // LoadSceneMode.Single means that all other scenes are unloaded before new scene is loaded.
        SceneManager.LoadScene(Scene, LoadSceneMode.Single); 
    }

    public string SerializedConfig()
    {
        return Config.Serialize();
    }

    public void StartBattery()
    {
        if (IsLoaded)
        {
            Config.Start();
        }
    }

    public void EndBattery()
    {
        if (IsLoaded)
        {
            Config.End();
        }
    }
   
    public string GetStartTime()
    {
        return Config.StartTime();
    }
    
    public string GetEndTime()
    {
        return Config.EndTime();
    }

    public void LoadNextScene()
    {
        // Scenes are indexed according to the order they appear in the battery config games list. The earlier in the list the earlier they will be loaded.
        if (IsLoaded)
        {
            Scene.Next();
            Debug.Log(Scene.Name());
            LoadScene(Scene.Name());
        }
    }

    private string GetCurrentScene()
    {  
        return Scene.Name();
    }

    // Lists the games that player will play during the battery session. Undecided if it will be a more than just useful for debugging.
    public List<string> GetGameList()
    {
        if (IsLoaded)
        {
            return Config.GameScenes();
        }
        return null;
    }

    // As the configurable variables are added, deleted or renamed during development in order not have to constantly sync these names with the configuration files this function can be used to generate a blank configuration file based off those variables. 
    public void WriteExampleConfig()
    {
        // TODO: FileHandle.WriteGenerated(Config.Generate());
    }
}
