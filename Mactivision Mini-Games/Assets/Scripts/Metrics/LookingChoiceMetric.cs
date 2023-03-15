using System;
using Newtonsoft.Json.Linq;

public class LookingChoiceMetric : AbstractMetric<LookingChoiceEvent>
{
	public LookingChoiceMetric()
	{
	}

    public override JObject getJSON()
    {
        JObject json = new JObject();

        json["metricName"] = JToken.FromObject("lookingChoice");
        json["eventList"] = JToken.FromObject(this.eventList);
        return json;
    }
}

