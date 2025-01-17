using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

// PositionMetric class records PositionEvents which occur during a game.
public class PositionMetric : AbstractMetric<PositionEvent> {

    // List of the (arbitrary) names of objects to track positions of
    public List<string> gameObjectKeys { get; }

    public PositionMetric(List<string> gameObjectKeys) {
        this.gameObjectKeys = gameObjectKeys;
    }

    public override JObject getJSON() {
        JObject json = new JObject();

        json["metricName"] = JToken.FromObject("position");
        json["gameObjectKeys"] = JToken.FromObject(this.gameObjectKeys);
        JArray jsonEvents = new JArray();
        foreach (PositionEvent e in this.eventList) {
            JObject jsonEvent = new JObject();
            jsonEvent["eventTime"] = JToken.FromObject(e.eventTime);
            JArray jsonPositions = new JArray();
            foreach (Vector2 v in e.positions) {
                JObject jsonPosition = new JObject();
                jsonPosition["x"] = v.x;
                jsonPosition["y"] = v.y;
                jsonPositions.Add(jsonPosition);
            }
            jsonEvent["positions"] = jsonPositions;
            jsonEvents.Add(jsonEvent);
        }
        json["eventList"] = jsonEvents;
        return json;
    }
}