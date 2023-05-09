using System;
using Newtonsoft.Json.Linq;

public class RecipeChoiceMetric : AbstractMetric<RecipeChoiceEvent>
{
    public RecipeChoiceMetric()
    {
    }

    public override JObject getJSON()
    {
        JObject json = new JObject();

        json["metricName"] = JToken.FromObject("recipeChoice");
        json["eventList"] = JToken.FromObject(this.eventList);
        return json;
    }
}

