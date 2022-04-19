using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class BenchmarkFun : MonoBehaviour
{
    public Vector3 member;
    private Vector3 test;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H)) {
            TestA();
        }
        if (Input.GetKeyDown(KeyCode.G)) {
            TestB();
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            TestC();
        }
        if (Input.GetKeyDown(KeyCode.K)) {
            TestD();
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            TestE();
        }
        if (Input.GetKeyDown(KeyCode.V)) {
            TestF();
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            TestG();
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            TestH();
        }
        if(Input.GetKeyDown(KeyCode.M)) {
            TestI();
        }
        if (Input.GetKeyDown(KeyCode.Comma)) {
            TestJ();
        }
    }


    void TestA() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for(UInt64 i = 0; i < 1000000; i++) {
            member = ModifyA(member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test A took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestB() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            member = ModifyB(member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test B took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestC() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            test = member;
            ModifyC(test,out member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test C took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestD() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            member = ModifyD(member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test D took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestE() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            ModifyE(ref member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test E took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestF() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            ModifyF(in member, out member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test F took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestG() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            test = member;
            ModifyG(ref member);
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test G took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestH() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            test.x = (member.x * 2 + 1) % 57;
            test.y = (member.y + member.x + 5) % 109;
            test.z = (member.z + member.x + member.y + 3) % 97;
            member = test;
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test H took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestI() {
        member = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            member.z = (member.z + member.x + member.y + 3) % 97;
            member.y = (member.y + member.x + 5) % 109;
            member.x = (member.x * 2 + 1) % 57;
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test I took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + member.x);
    }

    void TestJ() {
        Vector3 local = Vector3.zero;
        Stopwatch clock = new Stopwatch();
        clock.Start();
        for (UInt64 i = 0; i < 1000000; i++) {
            local.z = (local.z + local.x + local.y + 3) % 97;
            local.y = (local.y + local.x + 5) % 109;
            local.x = (local.x * 2 + 1) % 57;
        }
        clock.Stop();
        UnityEngine.Debug.Log("Test J took " + clock.ElapsedMilliseconds);
        UnityEngine.Debug.Log("member values are " + local.x);
    }

    //use a private class member as buffer
    Vector3 ModifyA(Vector3 vec) { //Faster
        test.x = (vec.x * 2 + 1) % 57;
        test.y = (vec.y + vec.x + 5) % 109;
        test.z = (vec.z + vec.x + vec.y + 3) % 97;
        return test;
    }

    //use local variable as buffer
    Vector3 ModifyB(Vector3 vec) { //Slower
        Vector3 res = new Vector3((vec.x * 2 + 1) % 57, (vec.y + vec.x) % 109, (vec.z + vec.x + vec.y + 3) % 97);
        return res;
    }

    //use the out parameter as a buffer (private class member)
    void ModifyC(Vector3 vec, out Vector3 res) { //Slower
        res.x = (vec.x * 2 + 1) % 57;
        res.y = (vec.y + vec.x + 5) % 109;
        res.z = (vec.z + vec.x + vec.y + 3) % 97;
    }

    //use a local variable as buffer without a constructor
    Vector3 ModifyD(Vector3 vec) { //Slowest
        Vector3 res = Vector3.zero;
        res.x = (vec.x * 2 + 1) % 57;
        res.y = (vec.y + vec.x) % 109;
        res.z = (vec.z + vec.x + vec.y + 3) % 97;
        return res;
    }

    //use the private class member as a buffer
    void ModifyE(ref Vector3 vec) {
        test.x = (vec.x * 2 + 1) % 57;
        test.y = (vec.y + vec.x + 5) % 109;
        test.z = (vec.z + vec.x + vec.y + 3) % 97;
        vec = test;
    }
        
    void ModifyF(in Vector3 vec, out Vector3 res) { //Much Faster
        res.x = (vec.x * 2 + 1) % 57;
        res.y = (vec.y + vec.x + 5) % 109;
        res.z = (vec.z + vec.x + vec.y + 3) % 97;
    }


    void ModifyG(ref Vector3 vec) { //Fastest but inplace (not possible all the time)
        vec.z = (vec.z + vec.x + vec.y + 3) % 97;
        vec.y = (vec.y + vec.x + 5) % 109;
        vec.x = (vec.x * 2 + 1) % 57;
    }
    /**
     * Heap access is faster than a Stack alloc
     * (or so I guess, not sure it is the case elsewhere)
     * and then return is slower than using out parameters
     * will try to use those over stack alloc from now
     * 
     **/
}
