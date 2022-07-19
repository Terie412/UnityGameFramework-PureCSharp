using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestCharacterBehaviour : MonoBehaviour
{
    public Vector2 velocity; // 速度
    public float radius;

    void FixedUpdate()
    {
        velocity = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            velocity.y = 1;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            velocity.y = -1;
        }
        
        if (Input.GetKey(KeyCode.A))
        {
            velocity.x = -1;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            velocity.x = 1;
        }
        
        Move(Time.fixedDeltaTime);
    }

    // 根据当前速度，运动时间 t
    void Move(float t, int count = 0)
    {
        if (Mathf.Abs(t - 0) <= float.Epsilon) return;  // 运行时间太短
        if (velocity.Equals(Vector2.zero)) return;      // 速度太小
        if (count > 10) return;                         // 迭代次数太多
        
        Vector2 posOrigin = transform.position;
        Vector2 offset = t * velocity;
        // Debug.Log($"从当前位置 {posOrigin}，半径 {radius}，方向 {velocity} 检测长度为 {(offset).magnitude} 的碰撞体");

        RaycastHit2D[] hits = Physics2D.CircleCastAll(posOrigin, radius, velocity, offset.magnitude);

        // 没有碰撞
        if (hits.Length == 0)
        {
            transform.position += (Vector3) offset;
            return;
        }

        // 未来有碰撞
        List<RaycastHit2D> allHits = hits.ToList();             // 所有的待检测的碰撞体
        List<RaycastHit2D> contactList = new(allHits.Count);    // 所有当前接触的碰撞体
        
        // 按照距离排序碰撞体
        allHits.Sort((hit1, hit2) =>
        {
            if (Math.Abs(hit1.distance - hit2.distance) < float.Epsilon) return 0;
            if (hit1.distance < hit2.distance) return -1;
            return 1;
        });

        int iterationCount = 0;
        while (true)
        {
            iterationCount++;
            if (iterationCount > 5)
            {
                Debug.LogError($"while 超过了最大的迭代速度");
                return;
            }
            
            contactList.Clear();
            RaycastHit2D firstHitUncontact = new RaycastHit2D();    // 第一个未接触的碰撞体
            
            // 获取所有接触的碰撞体
            for (var i = 0; i < allHits.Count; i++)
            {
                var hit = allHits[i];
                if (Mathf.Abs(hit.distance - 0) < float.Epsilon)
                {
                    contactList.Add(allHits[i]);
                }
                else if (firstHitUncontact.collider == null)
                {
                    firstHitUncontact = hit;
                }
            }

            if (contactList.Count == 0)
            {
                // 没有接触的碰撞体，直接运动到下一个碰撞体
                if (firstHitUncontact.collider == null)
                {
                    Debug.Log($"1111");
                    transform.position += (Vector3) offset;
                    break;
                }
                else
                {
                    Debug.Log($"2222 ： {iterationCount}");
                    transform.position = firstHitUncontact.centroid;
                
                    // 修正时间
                    float t2 = (posOrigin - (Vector2) transform.position).magnitude / offset.magnitude * t;
                
                    // 修正速度
                    velocity = velocity - Vector2.Dot(velocity, firstHitUncontact.normal) * firstHitUncontact.normal;
                
                    // 下一次迭代
                    Move(t2);
                    break;
                }
            }
            else if (contactList.Count == 1)
            { 
                Debug.Log($"3333 ： {iterationCount}");
                // 只有一个接触体，此时我们先修正速度，然后视该碰撞体不存在，再做一个运行预测
                RaycastHit2D hit = contactList[0];
                transform.position = hit.centroid;
                if (Vector2.Dot(velocity, hit.normal) < 0)
                {
                    velocity = velocity - Vector2.Dot(velocity, hit.normal) * hit.normal;
                }

                offset = velocity * t;
                allHits.Remove(hit);
            }
            else
            {
                break;
                // 存在碰撞的物体，排序所有的法线，找出 n1, n2
                // List<RaycastHit2D> hitLeftList = new List<RaycastHit2D>(contactList.Count);
                // List<RaycastHit2D> hitRightList = new List<RaycastHit2D>(contactList.Count);
                //
                // for (int i = 0; i < contactList.Count; i++)
                // {
                //     var hit = contactList[i];
                //     float angle = Vector2.SignedAngle(velocity, hit.normal);
                //     if (angle >= 0)
                //     {
                //         hitLeftList.Add(hit);
                //     }
                //     else
                //     {
                //         hitRightList.Add(hit);
                //     }
                // }
                //
                // hitLeftList.Sort((hit1, hit2) =>
                // {
                //     float angle1 = Mathf.Abs(Vector2.SignedAngle(velocity, hit1.normal));
                //     float angle2 = Mathf.Abs(Vector2.SignedAngle(velocity, hit2.normal));
                //
                //     if (angle1 < angle2)
                //     {
                //         return -1;
                //     }
                //
                //     if (angle1 == angle2)
                //     {
                //         return 0;
                //     }
                //
                //     return 1;
                // });
                //
                // hitRightList.Sort((hit1, hit2) =>
                // {
                //     float angle1 = Mathf.Abs(Vector2.SignedAngle(velocity, hit1.normal));
                //     float angle2 = Mathf.Abs(Vector2.SignedAngle(velocity, hit2.normal));
                //
                //     if (angle1 < angle2)
                //     {
                //         return -1;
                //     }
                //
                //     if (angle1 == angle2)
                //     {
                //         return 0;
                //     }
                //
                //     return 1;
                // });
                //
                // Vector2 n1 = hitLeftList[hitLeftList.Count].normal;
                // Vector2 n2 = hitRightList[hitRightList.Count].normal;
                //
                // if (Vector2.Dot(n1, n2) < 0)
                // {
                //     // 被彻底锁死
                //     return;
                // }
                //
                // float angle1 = Vector2.Angle(velocity, n1);
                // float angle2 = Vector2.Angle(velocity, n2);
                //
                // RaycastHit2D hitFinal = angle1 <= angle2 ? hitLeftList[hitLeftList.Count] : hitRightList[hitRightList.Count];
                //
                // // 修正速度
                // velocity = velocity - Vector2.Dot(velocity, hitFinal.normal) * hitFinal.normal;
                //
            }
            
        }
    }
}