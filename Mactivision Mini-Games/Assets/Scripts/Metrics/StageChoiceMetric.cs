using System;
using Newtonsoft.Json.Linq;

public class StageChoiceMetric : AbstractMetric<StageChoiceEvent>
{
    public StageChoiceMetric()
    {
    }

    public override JObject getJSON()
    {
        JObject json = new JObject();

        json["metricName"] = JToken.FromObject("stageChoice");
        json["eventList"] = JToken.FromObject(this.eventList);
        return json;
    }
}

