using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ProjectileShoot : MonoBehaviour
{
    public GameObject pfBullet;
    public GameObject pfMarker;
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
                GameObject trailMarker = Instantiate(pfMarker,trackedBullet.position,trackedBullet.rotation);
                Destroy(trailMarker,1f);
            } else {
                posHistory.Clear();
            }
            deltaCount=0;
        }
        float elevation = -0.5f;
        if(posHistory.Count>0) {
            Vector2 predictedPosition = predictPosition(elevation);
            if(!float.IsNaN(predictedPosition.magnitude))
                predIndicator.position=new Vector3(predictedPosition.x,elevation,predictedPosition.y);
        }
        
    }

    private void shootBullet() {
        GameObject newBullet = Instantiate(pfBullet,fireRef.position,fireRef.rotation);
        trackedBullet = newBullet.GetComponent<Rigidbody>();
        posHistory.Clear();
        trackedBullet.velocity = newBullet.transform.rotation*Vector3.forward*10+new Vector3(Random.value,Random.value,Random.value);
        Destroy(newBullet,5f);
    }
    //returns the x,z coordinates where y=elevation for the parabola y=f(x), or y=f(z) if useZ is true, and linear relationship between x and z
    //uses a quadratic equation to model the flight, which is accurate enough for objects whose drag decceleration isn't too large
    private Vector2 predictPosition(float elevation=0) {
        Vector2 direction = linReg(posHistory);//direction.x is the slope and direction.y is the y-intercept
        Debug.DrawLine(new Vector3(0,1,direction.y),new Vector3(1,1,direction.y+direction.x),new Color(255,0,0));
        bool useZ = direction.x>1;//if the slope is >1, then the change in z is greater so the regression will be better(I think, just on intuition)
        Vector3 parabola = quadReg(posHistory,useZ);
        //solve for zeros of parabola and then compute the remaining horizontal coordinate(either x or z)
        float a = parabola.x;
        float b = parabola.y;
        float c = parabola.z+elevation;
        float zero1 = (-b+Mathf.Sqrt(b*b-4*a*c))/(2*a);
        float zero2 = (-b-Mathf.Sqrt(b*b-4*a*c))/(2*a);
        float zero;
        if(useZ){
            zero = Mathf.Abs(zero1-transform.position.z)>Mathf.Abs(zero2-transform.position.z) ? zero1:zero2;//chooses the zero furthest from the launch point using the z coords
            return new Vector2((zero-direction.y)/direction.x,zero);//x and z coordinates of where the projectile lands
        } else {
            zero = Mathf.Abs(zero1-transform.position.x)>Mathf.Abs(zero2-transform.position.x) ? zero1:zero2;//chooses the zero furthest from the launch point using the x coords
            return new Vector2(zero,zero*direction.x+direction.y);//x and z coordinates of where the projectile lands
        }
    }

    private Vector2 linReg(LinkedList<Vector3> points) {//computes the regression between x and z, where z=ax+b
        //naive algorithm for computing mean and sample variance (it sucks because there are two loops). TODO: figure out something like Welford's Algorithm(https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance) except it has to subtract from the agregate was well, or maybe just don't subtract and
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
