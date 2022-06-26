using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        int[] a = {5, 4, -1, 7, 8};
        Debug.Log($"{MaxSubArray(a)}");
    }
    
    public int MaxSubArray(int[] nums)
    {
        int res = nums[0];
        int sum = 0;
        for (int i = 0;i < nums.Length;i++) {
            int num = nums[i];
            if (sum > 0)
                sum += num;
            else
                sum = num;
            res = Math.Max(res, sum);
        }
        return res;
    }
}