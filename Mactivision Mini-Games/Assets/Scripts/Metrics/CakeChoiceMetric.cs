using System;
using Newtonsoft.Json.Linq;

public class CakeChoiceMetric : AbstractMetric<CakeChoiceEvent>
{
    public CakeChoiceMetric()
    {
    }

    public override JObject getJSON()
    {
        JObject json = new JObject();

        json["metricName"] = JToken.FromObject("cakeChoice");
        json["eventList"] = JToken.FromObject(this.eventList);
        return json;
    }
}

