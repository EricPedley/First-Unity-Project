using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ProjectileShoot : MonoBehaviour
{
    public GameObject pfBullet;
    public Transform predIndicator;
    public Transform fireRef;
    private Rigidbody trackedBullet;
    private int deltaCount=0, historyLen=20;
    private LinkedList<Vector3> posHistory;
    private Vector2 movementDirection;
    // Start is called before the first frame update
    void Start()
    {
        posHistory = new LinkedList<Vector3>();
        posHistory.AddFirst(new Vector3(1,0,2));
        posHistory.AddFirst(new Vector3(2,0,3.8f));
        posHistory.AddFirst(new Vector3(2.9f,0,6.1f));
        Vector2 ab = linReg(posHistory);
        Debug.Log($"a: {ab.x}, b: {ab.y}");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            shootBullet();
        }
        deltaCount++;
        if(deltaCount>5) {
            if(trackedBullet!=null) {
                if(posHistory.Count==historyLen) {
                    posHistory.RemoveFirst();
                }
                posHistory.AddLast(trackedBullet.position);
            } else {
                posHistory.Clear();
            }
            Debug.Log(posHistory.Count);
            deltaCount=0;
        }
        float elevation = -0.5f;
        Vector2 predictedPosition = predictPosition(elevation);
        if(!float.IsNaN(predictedPosition.magnitude))
            predIndicator.position=new Vector3(predictedPosition.x,elevation,predictedPosition.y);
    }

    private void shootBullet() {
        GameObject newBullet = Instantiate(pfBullet,fireRef.position,fireRef.rotation);
        trackedBullet = newBullet.GetComponent<Rigidbody>();
        posHistory.Clear();
        trackedBullet.velocity = newBullet.transform.rotation*Vector3.forward*10+new Vector3(Random.value,Random.value,Random.value);
    }
    //returns the x,z coordinates where y=elevation for the parabola y=f(x) and xz direction, or y=f(z) if useZ is true
    private Vector2 predictPosition(float elevation=0,bool useZ=false) {
        Vector2 direction = linReg(posHistory);
        Vector3 parabola = quadReg(posHistory,direction.y>direction.x);
        //solve for zeros of parabola and then compute z coord.
        float a = parabola.x;
        float b = parabola.y;
        float c = parabola.z+elevation;
        float zero1 = (-b+Mathf.Sqrt(b*b-4*a*c))/(2*a);
        float zero2 = (-b-Mathf.Sqrt(b*b-4*a*c))/(2*a);
        float zero;
        if(useZ){
            zero = Mathf.Abs(zero1-posHistory.Last.Value.z)<Mathf.Abs(zero2-posHistory.Last.Value.z) ? zero1:zero2;//chooses the zero closest to the last recorded position of the projectile
            return new Vector2(zero,zero*direction.x/direction.y);//x and z coordinates of where the projectile lands
        } else {
            //BUG nullReferenceException on following line. Maybe Value.x because it's inserting null vectors in Update?
            zero = Mathf.Abs(zero1-posHistory.Last.Value.x)<Mathf.Abs(zero2-posHistory.Last.Value.x) ? zero1:zero2;//chooses the zero closest to the last recorded position of the projectile
            return new Vector2(zero,zero*direction.y/direction.x);//x and z coordinates of where the projectile lands
        }
    }

    private Vector2 linReg(LinkedList<Vector3> points) {//computes the regression between x and z, where z=ax+b
        //naive algorithm for computing mean and sample variance. TODO: figure out something like Welford's Algorithm(https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance) except it has to subtract from the agregate was well, or maybe just don't subtract and
        float sumX=0,sumZ=0,difSqSumX=0,difSqSumZ=0;
        foreach(Vector3 p in points) {
            sumX+=p.x;
            sumZ+=p.z;
        }
        float meanX = sumX/points.Count;
        float meanZ = sumZ/points.Count;
        foreach(Vector3 p in points) {
            difSqSumX+=Mathf.Pow(p.x-meanX,2);
            difSqSumZ+=Mathf.Pow(p.z-meanZ,2);
        }
        float a = Mathf.Sqrt(difSqSumZ/difSqSumX);//equivalent to the ratio of z's standard deviation to x's standard deviation
        float b = meanZ-a*meanX;
        return new Vector2(a,b);
    }
    //uses y=f(z) if useZ is true, otherwise uses y=f(x)
    //https://www.easycalculation.com/statistics/learn-quadratic-regression.php
    private Vector3 quadReg(LinkedList<Vector3> points,bool useZ = false) {
        float sumN=0,sumX=0,sumX2=0,sumX3=0,sumX4=0,sumXY=0,sumX2Y=0,sumY=0;
        foreach(Vector3 p in points) {
            sumN++;
            float val;
            if(useZ) {
                val=p.z;
            } else {
                val=p.x;
            }
            sumX+=val;
            sumX2+=val*val;
            sumX3+=val*val*val;
            sumX4+=val*val*val*val;
            sumY+=p.y;
            sumXY+=val*p.y;
            sumX2Y+=val*val*p.y;
        }
        float a = (sumX2Y*sumX2-sumXY*sumX3)/(sumX2*sumX4-sumX3*sumX3);
        float b = (sumXY*sumX4-sumX2Y*sumX3)/(sumX2*sumX4-sumX3*sumX3);
        float c = (sumY/sumN-b*sumX/sumN)-a*sumX2/sumN;
        //TODO, make this function return a,b, and c for quadratic ax^2+bx+c that fits the data in the points list, but with the x and z coordinates projected along direction
        //alternate idea: don't do the projection and instead just save timestamps on each coord and then do regressions on x,y,and z independently as functions of t.
        //or, instead of y being a function of the projection, make it a function of x or z, based on which changes more
        return new Vector3(a,b,c);
    }

}
