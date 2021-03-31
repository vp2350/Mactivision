using System.Collections;
using System.Collections.Generic;
using System;
using System.Windows;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Newtonsoft.Json.Linq;

public class TestPositionMetric
{
    [Test]
    public void TestPositionEvent()
    {
        System.DateTime dt1 = new System.DateTime(1977, 5, 25);
        Vector2 rockstar1 = new Vector2(0, 0);
        Vector2 light1 = new Vector2(10, 10);
        List<Vector2> p1 = new List<Vector2>();
        p1.Add(rockstar1);
        p1.Add(light1);
        PositionEvent pe1 = new PositionEvent(dt1, p1);
        Assert.IsNotNull(pe1);
        Assert.AreEqual(new System.DateTime(1977, 5, 25), pe1.eventTime);
        Assert.AreEqual(p1, pe1.positions);

        // Test creating position event with other reasonable inputs
        System.DateTime dt2 = new System.DateTime(2021, 2, 1, 4, 13, 12, 140);
        Vector2 rockstar2 = new Vector2(10, 10);
        Vector2 light2 = new Vector2(0, 0);
        List<Vector2> p2 = new List<Vector2>();
        p2.Add(rockstar2);
        p2.Add(light2);
        PositionEvent pe2 = new PositionEvent(dt2, p2);
        Assert.IsNotNull(pe2);
        Assert.AreEqual(new System.DateTime(2021, 2, 1, 4, 13, 12, 140), pe2.eventTime);
        Assert.AreEqual(p2, pe2.positions);;

        // Test creating position event with minimum datetime
        //System.DateTime dtmin = System.DateTime.MinValue;
        //PositionEvent pe3 = new PositionEvent(dtmin, new List<Vector2>(new Vector2(10, 20), new Vector2(20,10)));
        //Assert.IsNotNull(pe3);
        //Assert.AreEqual(System.DateTime.MinValue, pe3.eventTime);

        // Test creating position event with maximum datetime
        //System.DateTime dtmax = System.DateTime.MaxValue;
        //PositionEvent pe4 = new PositionEvent(dtmax, new Vector2(new Vector2(100, 100), new Vector2(0, 0)));
        //Assert.IsNotNull(pe4);
        //Assert.AreEqual(System.DateTime.MaxValue, pe4.eventTime);
    }

    [Test]
    public void TestConstructor()
    {
        PositionMetric pm = new PositionMetric(new List<string> {"1", "2"});
        Assert.IsFalse(pm.isRecording);
        Assert.IsNotNull(pm.eventList);
        Assert.IsEmpty(pm.eventList);
    }

    [Test]
    public void TestStartFinishRecording()
    {
        PositionMetric pm = new PositionMetric(new List<string> {"1", "2"});
        Assert.IsFalse(pm.isRecording);

        pm.startRecording();
        Assert.IsTrue(pm.isRecording);

        pm.finishRecording();
        Assert.IsFalse(pm.isRecording);
    }

    [Test]
    public void TestRecordEvent()
    {
        PositionMetric pm1 = new PositionMetric(new List<string> {"1", "2"});
        pm1.startRecording();
        Assert.IsEmpty(pm1.eventList);
        Assert.IsTrue(pm1.isRecording);
        
        pm1.recordEvent(null);
        Assert.IsEmpty(pm1.eventList);

        // Test adding events to a PositionMetric which has not started recording
        PositionMetric pm2 = new PositionMetric(new List<string> {"1", "2"});
        Assert.IsFalse(pm2.isRecording);
        Assert.IsEmpty(pm2.eventList);

        List<Vector2> v1 = new List<Vector2>();
        v1.Add(new Vector2(10, 10));
        v1.Add(new Vector2(0, 0));
        pm2.recordEvent(new PositionEvent(new System.DateTime(2021, 2, 1), v1));
        Assert.IsEmpty(pm2.eventList);

        //Test adding valid events to a PositionMetric which has started recording
        // PositionMetric pm3 = new PositionMetric(new List<string> {"1", "2"});
        // pm3.startRecording();
        // Assert.IsTrue(pm3.isRecording);
        // Assert.IsEmpty(pm3.eventList);

        // pm3.recordEvent(new PositionEvent(new System.DateTime(2021, 2, 1), new Vector2(new Vector2(100, 100), new Vector2(0, 0))));
        // pm3.recordEvent(new PositionEvent(new System.DateTime(2021, 2, 2), new Vector2(new Vector2(0, 0), new Vector2(20, 20))));
        // pm3.recordEvent(new PositionEvent(new System.DateTime(2021, 2, 3), new Vector2(new Vector2(20, 20), new Vector2(20, 30))));
        
        // Assert.AreEqual(3, pm3.eventList.Count);
        // Assert.AreEqual(new System.DateTime(2021, 2, 1), pm3.eventList[0].eventTime);
        // Assert.AreEqual(new Vector2(new Vector2(100, 100), new Vector2(0, 0)), pm3.eventList[0].positions);

        // Assert.AreEqual(new System.DateTime(2021, 2, 2), pm3.eventList[1].eventTime);
        // Assert.AreEqual(new Vector2(new Vector2(0, 0), new Vector2(20, 20)), pm3.eventList[1].positions);

        // Assert.AreEqual(new System.DateTime(2021, 2, 3), pm3.eventList[2].eventTime);
        // Assert.AreEqual(new Vector2(new Vector2(20, 20), new Vector2(20, 30)), pm3.eventList[2].positions);
    }

    [Test]
    public void TestGetJSON()
    {
        // Test getJSON() when metric has no records
        PositionMetric pm1 = new PositionMetric(new List<string> {"1", "2"});
        Assert.IsEmpty(pm1.eventList);

        JObject json1 = pm1.getJSON();
        Assert.IsNotNull(json1);

        // Test getJSON() with one record
        PositionMetric pm2 = new PositionMetric(new List<string> {"1", "2"});
        pm2.startRecording();
        Assert.IsTrue(pm2.isRecording);
        Assert.IsEmpty(pm2.eventList);

        List<Vector2> v1 = new List<Vector2>();
        v1.Add(new Vector2(20, 20));
        v1.Add(new Vector2(20, 30));
        pm2.recordEvent(new PositionEvent(new System.DateTime(2021, 2, 1), v1));
        pm2.finishRecording();

        JObject json2 = pm2.getJSON(); 
        Assert.AreEqual(1, pm2.eventList.Count);
        Assert.IsNotNull(json2);
        
        JArray json2p = (JArray) json2["eventList"];
        Assert.AreEqual(1, json2p.Count);
        Assert.AreEqual("2/1/2021 12:00:00 AM", json2p[0]["eventTime"].ToString());
    }
}