using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TrueSync;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GameMain_TrueSync : MonoBehaviour
{
    public GameObject go1;
    public GameObject go2;
    public Text text;

    private void Start()
    {
        Application.targetFrameRate = 30;

        // long a = 0b0111111111111111111111111111111111111000000000000000000000000000;
        // Debug.Log($"{Convert.ToString(a, 2)}");
        // Debug.Log($"{Convert.ToString((int)a, 2)}");


        // long factor = 1L << 32;
        // int a = 2147483647;
        // long b = a * factor;
        // double c = b;
        // float d = (float) (c / factor);
        // Debug.Log($"factor={factor}\na = {a}\nb={b}\nc={c:F}\nd={d:F}");
        // Debug.Log($"double表示：factor={Convert.ToString(factor, 2)}\na = {Convert.ToString(a, 2)}\nb={Convert.ToString(b, 2)}\nc={Convert.ToString((long)c, 2)}\nd={Convert.ToString((int)d, 2)}");

        // Debug.Log($"{FP.MaxValue}");
        // int a2 = 00;
        // float a3 = 2147483647;
        // Debug.Log($"{a3:F}");
        //
        // FP a = 2147483647;
        // text.text = $"a = {a}"; // 无论是在32bit和64bit的机器上（分别打32bit包和64bit的包），结果都是2147484000和-2147484000
        
        // a = 2147483646; //4294967296;
        // b = 2147483649; //4294967296;
        // text.text = $"a = {a}, b = {b}"; // 无论是在32bit和64bit的机器上（分别打32bit包和64bit的包），结果都是2147484000和-2147484000

        // long max = 1L << 30;
        // FP a = max;
        // long b = (long)a;
        // Debug.Log($"max = {max}, a = {a}, b = {b}, FP.max = {FP.MaxValue}");;
        
        long a = long.MaxValue;
        float b = a;
        Debug.Log($"a = {a}, b = {b}, offset = {a - b}, {float.MaxValue - long.MaxValue}");
    }
    
    private void TestMultiplePerformance()
    {
        List<FP> fps = new List<FP>();
        List<float> fs = new List<float>();

        int count = 10000000;
        for (int i = 0; i < count; i++)
        {
            float val = Random.Range(1, 10000000);
            fps.Add(val);
            fs.Add(val);
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < count; i+=2)
        {
            fps[i] *= fps[i + 1];
        }

        sw.Stop();
        Debug.Log($"FP cost {(double)sw.ElapsedMilliseconds / 1000} s");
        
        sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < count; i+=2)
        {
            fs[i] *= fs[i + 1];
        }

        sw.Stop();
        Debug.Log($"float cost {(double)sw.ElapsedMilliseconds / 1000} s");
        
        // 一千万次数下，float耗时 0.087 s, FP耗时 0.159 s，浮点求三角函数相差 1.827 倍
    }
    
    private void TestSinPerformance()
    {
        List<FP> fps = new List<FP>();
        List<float> fs = new List<float>();

        int count = 10000000;
        for (int i = 0; i < count; i++)
        {
            float val = Random.Range(1, 10000000);
            fps.Add(val);
            fs.Add(val);
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < count; i++)
        {
            fps[i] = FP.Sin(i);
        }

        sw.Stop();
        Debug.Log($"FP cost {(double)sw.ElapsedMilliseconds / 1000} s");
        
        sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < count; i++)
        {
            fs[i] = Mathf.Sin(fs[i]);
        }

        sw.Stop();
        Debug.Log($"float cost {(double)sw.ElapsedMilliseconds / 1000} s");
        
        // 一千万次数下，float耗时 0.512 s, FP耗时 1.729 s，浮点求三角函数相差 3.37 倍
    }

    private void TestSqrtPerformance()
    {
        List<FP> fps = new List<FP>();
        List<float> fs = new List<float>();

        int count = 10000000;
        for (int i = 0; i < count; i++)
        {
            float val = Random.Range(1, 10000000);
            fps.Add(val);
            fs.Add(val);
        }

        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < count; i++)
        {
            fps[i] = FP.Sqrt(fps[i]);
        }

        sw.Stop();
        Debug.Log($"FP cost {(double)sw.ElapsedMilliseconds / 1000} s");
        
        sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < count; i++)
        {
            fs[i] = Mathf.Sqrt(fs[i]);
        }

        sw.Stop();
        Debug.Log($"float cost {(double)sw.ElapsedMilliseconds / 1000} s");
        
        // 一千万次数下，float耗时 0.241 s, FP耗时 4.107 s，浮点求根号运算性能相差 17 倍
    }

    private void Do()
    {
        // go1.transform.GetComponent<Rigidbody>().AddForce();
    }
}