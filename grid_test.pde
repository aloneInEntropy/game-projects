import java.util.Arrays;
import java.util.Set;
import java.util.HashSet;
import java.util.Collections;
import java.io.File;

void settings() {
    fullScreen();
    // size(1024, 1024);
}

// !! IMPLEMENT DUMMY/ILLEGAL NODES

final int HEIGHT_LIMIT = height;
// Integer[] x_points = new Integer[]{ 30, 30, 30, 50, 50, 50, 70, 70, 70, 90, 90, 90, 110, 110, 110};
// Integer[] y_points = new Integer[]{ 20, 40, 60, 20, 40, 60, 20, 40, 60, 20, 40, 60, 20, 40, 60};
// Integer[] x_points = new Integer[]{ 30, 30, 30, 30, 50, 50, 50, 50, 50, 70, 70, 70, 70, 70, 90, 90, 90, 90, 90, 110, 110, 110, 110, 110, 130, 120, 130, 130, 150, 150};
// Integer[] y_points = new Integer[]{ 40, 60, 80, 100, 20, 40, 60, 80, 100, 20, 40, 60, 80, 100, 20, 40, 60, 80, 100, 40, 60, 120, 80, 100, 60, 100, 120, 80, 80, 60};
// Integer[] x_points = new Integer[]{ 100, 120, 120, 140, 140, 120, 120, 100, 100, 80, 80, 100};
// Integer[] y_points = new Integer[]{ 10, 10, 30, 30, 50, 50, 70, 70, 50, 50, 30, 30};
Integer[] x_points = new Integer[]{ };
Integer[] y_points = new Integer[]{ };
ArrayList<PVector> points = new ArrayList<>();
ArrayList<PVector> op = new ArrayList<>(); // outer points
ArrayList<PVector> closest = new ArrayList<>(); // points closer to target
ArrayList<PVector> dummy = new ArrayList<>(); // points not allowed to be drawn on or connected to
HashMap<PVector, PVector> filled = new HashMap<>(); // location of filled squares, the colour of said squares
Set<PVector> fill_nec = new HashSet<>(); // points that filled squares require when checking
Set<PVector> tba = new HashSet<>(); // points To Be Added (for drawing)
ArrayList<PVector> tbr = new ArrayList<>(); // points To Be Removed (for eraser)
Set<PVector> tbad = new HashSet<>(); // points To Be Added (Dummy points)
ArrayList<PVector> tbrd = new ArrayList<>(); // points To Be Removed (Dummy points)
int gap = 20; // grid size
int tx, ty;
int highest = Integer.MIN_VALUE, lowest = Integer.MAX_VALUE;
color mColour_E = color(255, 100, 0); // eraser colour
color mColour_D = color(0, 100, 255); // drawing colour
color mColour = color(0, 100, 255); // current colour (defaults to drawing)
int mOpacity = 100; // mouse cursor opacity
int mRadius = 15; // mouse cursor radius
boolean eraserMode = false; // will dragging the mouse erase points?
boolean dummyMode = false; // will dragging the mouse draw dummy points?
int eraserThreshold = 100; // max points to be erased before clearing tbr
String promptText = "";
PImage infection;
PVector target = new PVector();
boolean targetFollow = false;
boolean debug = false; // show debug info
int rLowerBound = 5, rMiddleBound = 10, rUpperBound = 100; // probability boundaries
int maxClosestPoints = 64; // max points allowed to spread

void setup() {
    frameRate(240);
    infection = loadImage("Infection.png");
    for (int i = 0; i < x_points.length; i++) {
        points.add(new PVector(x_points[i], y_points[i]));
    }
    
    // spawn point. all other points spawn from here.
    points.add(new PVector(roundToN((int)width / 2, gap), roundToN((int)2 * height / 3, gap)));
    op.add(new PVector(roundToN((int)width / 2, gap), roundToN((int)2 * height / 3, gap)));
    
    // target point. all points will prefer heading towards this point over others
    target = new PVector(roundToN((int)width / 5, gap), roundToN((int)height / 5, gap));
    // target = new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap));
}

void draw() {
    background(0);
    if (targetFollow) target = new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap));
    // else target = target;
    
    strokeWeight(3);
    fill(255,255,255);
    stroke(255);

    
    // boundary checks
    checkBoundary(points, op, gap);
    // ArrayList<PVector> tpoints = new ArrayList<>(points);
    // for (PVector p : tpoints) {
    //     if (op.size() >= maxClosestPoints && !op.contains(p) && filled.size() > 0 && !filled.containsKey(p)) points.remove(p);
    // }
    
    // PVector tpv = new PVector(10+gap*round(random(0, 200)/5), gap*round(random(0, 200)/5));
    // if (!points.contains(tpv)) points.add(tpv);
    
    // Spread controller.
    // Picks a random vector and creates a new point adjacent to it, pathfinding towards the target.
    if (!points.isEmpty()) {
        // if (!true) {
        PVector tpv = new PVector();
        if (!closest.isEmpty()) tpv = closest.get((int)random(0, closest.size()));
        else tpv = points.get((int)random(0, points.size()));
        int rnd_dir_x = (int)random(0, rUpperBound);
        int rnd_dir_y = (int)random(0, rUpperBound);
        int amnt_x = 0, amnt_y = 0;
        
        if (rnd_dir_x <= rLowerBound) {
            if (rnd_dir_x % 2 == 0) amnt_x = gap;
            else amnt_x = 0;
        } else if (rnd_dir_x <= rMiddleBound) {
            amnt_x = -gap;
        } else {
            if (target.x >= tpv.x) amnt_x = gap;
            else amnt_x = -gap;
        }
        
        if (rnd_dir_y <= rLowerBound) {
            if (rnd_dir_y % 2 == 0) amnt_y = gap;
            else amnt_y = 0;
        } else if (rnd_dir_y <= rMiddleBound) {
            amnt_y = -gap;
        } else {
            if (target.y >= tpv.y) amnt_y = gap;
            else amnt_y = -gap;
        }
        
        
        PVector npv = new PVector(roundToN((int)(tpv.x + amnt_x), gap), roundToN((int)(tpv.y + amnt_y), gap));
        // println("npv: "+npv);
        if (!points.contains(npv)) {
            // println("npv: "+npv);
            highest = (int)max(highest, npv.y);
            lowest = (int)min(lowest, npv.y);
            // println("highest: " + highest);
            // println("lowest: " + lowest);
            // op.add(npv);
            // op.remove(tpv);
            points.add(npv);
        }
    }    
    
    // recalculate the k nearest points to the target
    closest = findClosestPoints(op, target, maxClosestPoints);
    
    
    // draw the points to the screen
    fill(255,255,255);
    stroke(255,255,255);
    strokeWeight(1);
    // for (int i = 0; i < points.size(); i++) {
    //     ellipse(points.get(i).x, points.get(i).y, 5, 5);
    // }
    // for (int i = 0; i < op.size(); i++) {
    //     ellipse(op.get(i).x, op.get(i).y, 5, 5);
// }
    
    
    // draw the squares at points in the array points to the screen
    noStroke();
    for (int i = 0; i < points.size(); i++) {
        PVector pv = points.get(i);
        // ellipse(op.get(i).x, op.get(i).y, 5, 5);
        // check if all corners of tile are also within the grid
        PVector tcol = new PVector((int)random(30, 255),(int)random(30, 255),(int)random(30, 255)); 
        if (
            points.contains(new PVector(pv.x + gap, pv.y + gap)) && 
            points.contains(new PVector(pv.x + gap, pv.y)) && 
            points.contains(new PVector(pv.x, pv.y + gap)) && 
            !filled.containsKey(pv)
           ){
            // rect(pv.x, pv.y, gap, gap);
            filled.put(pv, tcol);
            fill_nec.add(pv);
            fill_nec.add(new PVector(pv.x + gap, pv.y + gap));
            fill_nec.add(new PVector(pv.x + gap, pv.y));
            fill_nec.add(new PVector(pv.x, pv.y + gap));
        }
    }
    
    // points = new ArrayList<>(fill_nec);
    
    for (PVector p : filled.keySet()) {
        fill(filled.get(p).x, filled.get(p).y, filled.get(p).z, 200);
        image(infection, p.x, p.y, gap, gap);
        rect(p.x, p.y, gap, gap);
    }
    
    for (PVector p : op) {
        fill(255,0,0);
        // ellipse(p.x, p.y, 10, 10);
    }
    
    for (PVector p : closest) {
        fill(0,0,255);
        // ellipse(p.x, p.y, 11, 11);
    }
    
    for (PVector p : dummy) {
        fill(255,160,0);
        ellipse(p.x, p.y, 11, 11);
    }
    
    // target point
    fill(0,255,0);
    ellipse(target.x, target.y, 30, 30);

    // temporarily draw all points using the mColour_D colour
    for (PVector p : tba) {
        fill(mColour);
        ellipse(p.x, p.y, 5, 5);
    }

    // temporarily draw all dummy points 
    for (PVector p : tbad) {
        fill(180, 0, 0);
        ellipse(p.x, p.y, 5, 5);
    }
    
    if (tbr.size() > eraserThreshold) tbr.clear();
    if (tbrd.size() > eraserThreshold) tbrd.clear();
    
    if (eraserMode) {
        mColour = mColour_E;
        promptText = "Press 'C' to switch to drawing mode.";
    }
    else {
        mColour = mColour_D;
        promptText = "Press 'C' to switch to eraser mode.";
    }
    
    // mouse indicator
    fill(mColour, mOpacity);
    ellipse(mouseX, mouseY, mRadius * 2, mRadius * 2);
    
    // textboxes
    fill(128, 0, 0);
    rect(0, height - height / 16, width / 5, height);
    rect(0, 0, width / 8, height / 20);
    
    // text
    fill(255);
    textSize(sqrt((width * width) + (height * height)) / 150);
    text(promptText, width / 25, height - height / 35); // prompt text - pen/eraser mode
    text("Press 'Q' to toggle target follow: " + (targetFollow ? "ON" : "OFF"), width / 25, height - height / 70); // prompt text - target follow
    text("Press 'D' to toggle dummy mode: " + (dummyMode ? "ON" : "OFF"), width / 25, height - height / 23); // prompt text - dummy mode

    if (debug) {
        text("FPS: " + round(frameRate), width / 25, height / 35); // fps
        text("Points: " + points.size(), width - width / 15, height / 60); // points
        text("Edge Points: " + op.size(), width - width / 15, height / 35); // bounding points
        text("Seeking Points: " + closest.size(), width - width / 15, height / 25); // (bounding) points that can spread
        text("Mouse: ", width - width / 15, height / 18.5); // mouse coordinates   
        text("X: " + roundToN(mouseX, gap) + "    Y: " + roundToN(mouseY, gap), width - width / 19, height / 15); // mouse coordinates   
        text("Target: ", width - width / 15, height / 12); // target coordinates   
        text("X: " + roundToN((int)target.x, gap) + "    Y: " + roundToN((int)target.y, gap), width - width / 19, height / 10); // target coordinates 
    }

    op.clear(); // reset outer edges and points to be recalculated on every loop  
}

boolean findVectorInArray(ArrayList<PVector> arr, PVector target) {
    // given a PVector and an array of PVectors, find the target PVector in the array.
    // returns true if `target` exists, and false otherwise
    
    for (int i = 0; i < arr.size(); i++) {
        if (arr.get(i).x == target.x && arr.get(i).y == target.y) return true;
    }
    return false;
}

boolean findVectorInSet(Set<PVector> arr, PVector target) {
    // given a PVector and a set of PVectors, find the target PVector in the set.
    // returns true if `target` exists, and false otherwise
    
    for (PVector pv : arr) {
        if (pv.x == target.x && pv.y == target.y) return true;
    }
    return false;
}

int roundToN(int x, int n) {
    int p = floor(x / n);
    return p * n;
}


void mouseDragged() {
    // println("mouseX: " + mouseX + "\nmouseY: " + mouseY);
    println("points " + points.size() + "\nouter points: " + op.size() + "\nfilled squares: " + filled.size());
    mOpacity = 255;
    
    if (eraserMode) {
        if (dummyMode) {
            for (PVector p : dummy) {
                if (dist(p.x, p.y, mouseX, mouseY) < mRadius) {
                    tbrd.add(p);
                }
            }
            dummy.removeAll(tbrd);
            
        } else {
            for (PVector p : points) {
                if (dist(p.x, p.y, mouseX, mouseY) < mRadius) {
                    tbr.add(p);
                }
            }
            
            points.removeAll(tbr);
            op.removeAll(tbr);
            for (PVector p : tbr) {
                filled.remove(p);
            }
        }
    } else {
        if (dummyMode) tbad.add(new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap)));
        else tba.add(new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap)));
    }
    
    
    
}

void mouseReleased() {
    mOpacity = 100;
    tbr.clear();
    points.addAll(tba);
    tba.clear();
    tbrd.clear();
    dummy.addAll(tbad);
    tbad.clear();
}


void keyPressed() {
    if (key == 'c' || key == 'C') {
        eraserMode = !eraserMode;
    } else if (key == 'd' || key == 'D') {
        dummyMode = !dummyMode;
    } else if (key == 'q' || key == 'Q') {
        targetFollow = ! targetFollow;
    } else if (key == 'a' || key == 'A') {
        maxClosestPoints = min(maxClosestPoints*2, 256);
    } else if (key == 's' || key == 'S') {
        maxClosestPoints = max(maxClosestPoints/2, 2);
    } else if (keyCode == DELETE) {
        points.clear();
        op.clear();
        filled.clear();
        closest.clear();
        fill_nec.clear();
        dummy.clear();
        // tbrd.clear();
        // tbad.clear();
    } else if (keyCode == ENTER || keyCode == RETURN) {
        debug = !debug;
    } else if (keyCode == UP) {
        for (PVector p : points) {
            PVector tp = new PVector(p.x, p.y - gap);
            points.set(points.indexOf(p), tp);
            if (filled.containsKey(p)) filled.remove(p);
        }
    } else if (keyCode == DOWN) {
        for (PVector p : points) {
            PVector tp = new PVector(p.x, p.y + gap);
            points.set(points.indexOf(p), tp);
            if (filled.containsKey(p)) filled.remove(p);
        }
    } else if (keyCode == LEFT) {
        for (PVector p : points) {
            PVector tp = new PVector(p.x - gap, p.y);
            points.set(points.indexOf(p), tp);
            if (filled.containsKey(p)) filled.remove(p);
        }
    } else if (keyCode == RIGHT) {
        for (PVector p : points) {
            PVector tp = new PVector(p.x + gap, p.y);
            points.set(points.indexOf(p), tp);
            if (filled.containsKey(p)) filled.remove(p);
        }
    }
}


// given a group of points and a target point, find the k closest points to the target
ArrayList<PVector> findClosestPoints(ArrayList<PVector> pts, PVector target, int k) {
    ArrayList<PVector> tp = new ArrayList<>(k);
    ArrayList<Float> dists = new ArrayList<>(k);
    PVector min = new PVector(Integer.MIN_VALUE, Integer.MIN_VALUE);
    int entered = 0;
    int i = 0;
    for (PVector p : pts) {
        if (entered >= k) {
            // if array is full
            float m = Collections.max(dists);
            // println("m: "+m);
            if (p.dist(target) < m) {
                // println("m: "+m);
                int ti = dists.indexOf(m);
                // println("ti: "+ti);
                dists.remove(m);
                tp.remove(ti);
                entered = k - 1;
            } else continue;
        } 
        tp.add(p);
        dists.add(p.dist(target));
        entered += 1;
    }
    // println("entered: "+entered);
    // println("size: "+tp.size());
    return tp;
}


void checkBoundary(ArrayList<PVector> pts, ArrayList<PVector> bp, int grid_distance) {
    for (int i = 0; i < pts.size(); i++) {
        int tx = (int)pts.get(i).x;
        int ty = (int)pts.get(i).y;
        
        if (
            // +hor and +ver line check
           (!findVectorInArray(pts, new PVector(tx + grid_distance, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty + grid_distance)))
           ) {
            // line(tx, ty, tx, ty + grid_distance); // down
            // line(tx, ty, tx + grid_distance, ty); // right
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
        }
        if (
            // +hor and -ver line check
           (!findVectorInArray(pts, new PVector(tx + grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty + grid_distance)))
           ) {
            // line(tx, ty, tx, ty - grid_distance); // up
            // line(tx, ty, tx + grid_distance, ty); // right
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
        }
        if (
            // -hor and +ver line check
           (!findVectorInArray(pts, new PVector(tx - grid_distance, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty - grid_distance)))
           ) {
            // line(tx, ty, tx, ty + grid_distance); // down
            // line(tx, ty, tx - grid_distance, ty); // left
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
        }
        if (
            // -hor and -ver line check
           (!findVectorInArray(pts, new PVector(tx - grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty - grid_distance)))
           ) {
            // line(tx, ty, tx, ty - grid_distance); // up
            // line(tx, ty, tx - grid_distance, ty); // left
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
        }
        if (
            // extra check
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty-grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty+grid_distance))) &&
           (!findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (!findVectorInArray(pts, new PVector(tx + grid_distance, ty+grid_distance))) && 
           (!findVectorInArray(pts, new PVector(tx, ty+grid_distance))) 
           ) {
            // line(tx, ty, tx, ty - grid_distance); // up
            // line(tx, ty, tx - grid_distance, ty); // left
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
        }
        if (
            // verticals check
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty)) ^
            findVectorInArray(pts, new PVector(tx - grid_distance, ty)))
           ) {
            // line(tx, ty, tx, ty + grid_distance); // down
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
            if (!bp.contains(new PVector(tx, ty + grid_distance))) bp.add(new PVector(tx, ty + grid_distance));
        }
        if (
            // horiontals check
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance)) ^
            findVectorInArray(pts, new PVector(tx, ty - grid_distance)))
           ) {
            // line(tx, ty, tx + grid_distance, ty); // right
            if (!bp.contains(new PVector(tx, ty))) bp.add(new PVector(tx, ty));
            if (!bp.contains(new PVector(tx + grid_distance, ty))) bp.add(new PVector(tx + grid_distance, ty));
        }   
    }
}