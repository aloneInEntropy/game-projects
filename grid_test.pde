import java.util.Arrays;
import java.util.Set;
import java.util.HashSet;
import java.util.Collections;


void settings() {
    fullScreen();
    // size(1024, 1024);
}

// !! IMPLEMENT BETTER PATHFINDING
// !! IMPLEMENT COMPARABLE CLASS (REPLACEMENT FOR PVECTOR)
// * the above goals are way too difficult for me to implement here, so i'll move on to GDScript now.



ArrayList<PVector> points = new ArrayList<>(); // points on grid
HashMap<PVector, ArrayList<PVector>> edges = new HashMap<>(); // edges in grid, with a from and to vector
ArrayList<PVector> op = new ArrayList<>(); // outer points
ArrayList<PVector> closest = new ArrayList<>(); // points closer to target
ArrayList<PVector> dummy = new ArrayList<>(); // points not allowed to be drawn on or connected to
HashMap<PVector, PVector> filled = new HashMap<>(); // location of filled squares, the colour of said squares
Set<PVector> fill_nec = new HashSet<>(); // points that filled squares require when checking
Set<PVector> tba = new HashSet<>(); // points To Be Added (for drawing)
ArrayList<PVector> tbr = new ArrayList<>(); // points To Be Removed (for eraser)
Set<PVector> tbad = new HashSet<>(); // points To Be Added (Dummy points)
ArrayList<PVector> tbrd = new ArrayList<>(); // points To Be Removed (Dummy points)
int gap = 32; // grid size
int tx, ty;
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
boolean ctrl = false; // is CTRL being pressed?
int rLowerBound = 5, rMiddleBound = 10, rUpperBound = 100; // probability boundaries
int maxClosestPoints = 64; // max points allowed to spread
int grid_interval = 1; // choose which points are allowed to spread along this interval
int pgi = 1; // previous grid interval

void setup() {
    // frameRate(240);
    
    // spawn point. all other points spawn from here.
    // points.add(new PVector(roundToN((int)width / 2, gap), roundToN((int)2 * height / 3, gap)));
    
    // target point. all points will prefer heading towards this point over others
    target = new PVector(roundToN((int)width / 10, gap), roundToN((int)height / 3, gap));
}

void draw() {
    background(0);
    if (targetFollow) target = new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap));
    
    strokeWeight(3);
    fill(255,255,255);
    stroke(255);
    
    // boundary checks
    checkBoundary(points, op, gap);
    
    // Spread controller.
    // Picks a random vector and creates a new point adjacent to it, pathfinding towards the target.
    if (!points.isEmpty()) {
        // if (op.isEmpty()) points = new ArrayList<>(pathfind(points, closest, target));
        // else {
        //     points = new ArrayList<>(bfsfull(edges, points, target));
        // }
        points = new ArrayList<>(pathfind(points, closest, target));
    }    

    if (points.contains(target)) grid_interval = min(grid_interval * 16, 512); // slow down if target is found
    else grid_interval = pgi;
    
    // recalculate the k nearest points to the target
    closest = findClosestPoints(op, target, maxClosestPoints);
    
    // draw the points to the screen
    fill(255,255,255);
    stroke(255,255,255);
    strokeWeight(1);
    
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
            // dummy.contains(new PVector(pv.x + gap, pv.y + gap)) && 
            // dummy.contains(new PVector(pv.x + gap, pv.y)) && 
            // dummy.contains(new PVector(pv.x, pv.y + gap)) && 
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
    
    for (PVector p : filled.keySet()) {
        if (p.x <= width - gap && p.x >= 0 && p.y <= height - gap && p.y >= 0) {
            fill(filled.get(p).x, filled.get(p).y, filled.get(p).z, 200 * int(!debug));
            // image(infection, p.x, p.y, gap, gap);
            rect(p.x, p.y, gap, gap);
        }
    }
    
    // for (PVector p : points) {
    //     if (p.x <= width - gap && p.x >= 0 && p.y <= height - gap && p.y >= 0) {
    //         fill(255,0,0);
    //         if (debug) ellipse(p.x, p.y, 10, 10);
    //     }
    // }
    
    for (PVector p : op) {
        if (p.x <= width - gap && p.x >= 0 && p.y <= height - gap && p.y >= 0) {
            fill(255,0,0);
            if (debug) ellipse(p.x, p.y, 10, 10);
        }
    }
    
    for (PVector p : closest) {
        if (p.x <= width - gap && p.x >= 0 && p.y <= height - gap && p.y >= 0) {
            fill(0,0,255);
            if (debug) ellipse(p.x, p.y, 11, 11);
        }
    }
    
    for (PVector p : dummy) {
        if (p.x <= width - gap && p.x >= 0 && p.y <= height - gap && p.y >= 0) {
            fill(255,160,0);
            ellipse(p.x, p.y, 11, 11);
        }
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
    
    for (PVector from : edges.keySet()) {
        for (PVector to : edges.get(from)) {
            stroke(10, 255, 40);
            line(from.x, from.y, to.x, to.y);
        }
    }
    
    if (tbr.size() > eraserThreshold) tbr.clear();
    if (tbrd.size() > eraserThreshold) tbrd.clear();
    
    if (eraserMode) {
        mColour = mColour_E;
        promptText = "Press 'C' to switch to drawing mode.";
    } else {
        mColour = mColour_D;
        promptText = "Press 'C' to switch to eraser mode.";
    }
    
    noStroke();
    float txtsz = sqrt((width * width) + (height * height)) / 150;
    float debug_start_x = width - width / 12;
    float debug_start_y = (height / 60) + 5;
    
    // mouse indicator
    fill(mColour, mOpacity);
    ellipse(mouseX, mouseY, mRadius * 2, mRadius * 2);
    
    // textboxes
    fill(128, 0, 0);
    rect(0, height - height / 10, width / 5, height);
    
    // text
    fill(255);
    textSize(txtsz);
    text("Press 'Q' to toggle target follow: " + (targetFollow ? "ON" : "OFF"), 
        width  - debug_start_x - txtsz * 10, height - debug_start_y - txtsz * 5); // prompt text - target follow
    text("Press 'D' to toggle dummy mode: " + (dummyMode ? "ON" : "OFF"), 
        width  - debug_start_x - txtsz * 10, height - debug_start_y - txtsz * 4); // prompt text - dummy mode
    text("Press 'A' to increase search range and 'S' to decrease.", 
        width  - debug_start_x - txtsz * 10, height - debug_start_y - txtsz * 3); // prompt text - maxClosestPoints
    text("Press 'K' to increase search speed and 'L' to decrease.", 
        width  - debug_start_x - txtsz * 10, height - debug_start_y - txtsz * 2); // prompt text - grid_interval
    text(promptText, width  - debug_start_x - txtsz * 10, height - debug_start_y - txtsz); // prompt text - pen/eraser mode
    
    if (debug) {
        text("Points: " + points.size(), debug_start_x, debug_start_y); // points
        text("Edge Points: " + op.size(), debug_start_x, debug_start_y + txtsz); // bounding points
        text("Seeking Points: " + closest.size(), debug_start_x, debug_start_y + txtsz * 2); // (bounding) points that can spread
        text("Mouse: ", debug_start_x, debug_start_y + txtsz * 3); // mouse coordinates   
        text("X: " + roundToN(mouseX, gap) + "    Y: " + roundToN(mouseY, gap), debug_start_x + txtsz, debug_start_y + txtsz * 4); // mouse coordinates   
        text("Target: ", debug_start_x, debug_start_y + txtsz * 5); // target coordinates   
        // target coordinates 
        text("X: " + roundToN((int)target.x, gap) + "    Y: " + roundToN((int)target.y, gap), debug_start_x + txtsz, debug_start_y + txtsz * 6); 
        text("FPS: " + round(frameRate), debug_start_x, debug_start_y + txtsz * 7); // fps
        text("Seek Speed: " + (int)(100 * (1.0 / grid_interval)) + "%", debug_start_x, debug_start_y + txtsz * 8); // seeking speed
        text("Eraser Mode: " + eraserMode, debug_start_x, debug_start_y + txtsz * 9); // eraser mode
        text("Dummy Mode: " + dummyMode, debug_start_x, debug_start_y + txtsz * 10); // dummy mode
        text("Target Follow: " + targetFollow, debug_start_x, debug_start_y + txtsz * 11); // target follow
        text("Edges: " + edges.size(), debug_start_x, debug_start_y + txtsz * 12); // target follow
    }
    
    // points = new ArrayList<>(op);
    // points.removeAll(op);
    
    // reset outer edges and points to be recalculated on every loop  
    edges.clear(); 
    op.clear(); 
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
    // println("points " + points.size() + "\nouter points: " + op.size() + "\nfilled squares: " + filled.size());
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
            filled.keySet().removeAll(tbr);
            edges.keySet().removeAll(tbr);
            // for (PVector p : tbr) {
            //     filled.remove(p);
        // }
        }
    } else {
        if (dummyMode) tbad.add(new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap)));
        else {
            tba.add(new PVector(roundToN(mouseX, gap), roundToN(mouseY, gap)));
        }
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
    for (PVector p : points) {
        if (filled.containsKey(p)) filled.remove(p);
    }
}


void keyPressed() {
    if (key == 'c' || key == 'C') {
        eraserMode = !eraserMode;
    } else if (key == 'd' || key == 'D') {
        dummyMode = !dummyMode;
    } else if (key == 'q' || key == 'Q') {
        targetFollow = !targetFollow;
    } else if (key == 'a' || key == 'A') {
        maxClosestPoints = min(maxClosestPoints * 2, 1024); // increase
    } else if (key == 's' || key == 'S') {
        maxClosestPoints = max(maxClosestPoints / 2, 2); // decrease
    } else if (key == 'l' || key == 'L') {
        grid_interval = min(grid_interval * 2, 256); // slow down
        pgi = grid_interval;
    } else if (key == 'k' || key == 'K') {
        grid_interval = max(grid_interval / 2, 1); // speed up
        pgi = grid_interval;
    } else if (keyCode == CONTROL) {
        ctrl = true;
    } else if (keyCode == DELETE) {
        if (ctrl) {
            dummy.clear();
        }
        points.clear();
        op.clear();
        filled.clear();
        closest.clear();
        fill_nec.clear();
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

void keyReleased() {
    if (keyCode == CONTROL) ctrl = false;
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
            if (p.dist(target) < m) {
                int ti = dists.indexOf(m);
                dists.remove(m);
                tp.remove(ti);
                entered = k - 1;
            } else continue;
        } 
        tp.add(p);
        dists.add(p.dist(target));
        entered += 1;
    }
    
    return tp;
}

void refreshPointsOnRelease(ArrayList<PVector> pts) {
    for (PVector p : pts) {
        if (filled.containsKey(p)) filled.remove(p);
    }
}


// all adjacent points as dictated by the map of edges
ArrayList<PVector> adj(PVector v, HashMap<PVector, ArrayList<PVector>> e) {
    return e.get(v);
}


void checkBoundary(ArrayList<PVector> pts, ArrayList<PVector> bp, int grid_distance) {
    for (int i = 0; i < pts.size(); i++) {
        PVector tpv = pts.get(i);
        int tx = (int)tpv.x;
        int ty = (int)tpv.y;
        ArrayList<PVector> tos;
        
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
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) {
                    line(tx, ty, tx, ty + grid_distance); // down
                    line(tx, ty, tx + grid_distance, ty); // right
                }
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx, ty + grid_distance));
                tos.add(new PVector(tx + grid_distance, ty));
                edges.put(tpv, tos);
            }
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
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) {
                    line(tx, ty, tx, ty - grid_distance); // up
                    line(tx, ty, tx + grid_distance, ty); // right
                }
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx, ty - grid_distance));
                tos.add(new PVector(tx + grid_distance, ty));
                edges.put(tpv, tos);
            }
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
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) {
                    line(tx, ty, tx, ty + grid_distance); // down
                    line(tx, ty, tx - grid_distance, ty); // left
                }
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx, ty + grid_distance));
                tos.add(new PVector(tx - grid_distance, ty));
                edges.put(tpv, tos);
            }
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
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) {
                    line(tx, ty, tx, ty - grid_distance); // up
                    line(tx, ty, tx - grid_distance, ty); // left
                }
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx, ty - grid_distance));
                tos.add(new PVector(tx - grid_distance, ty));
                edges.put(tpv, tos);
            }
        }
        if (
            // extra check
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty - grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx - grid_distance, ty + grid_distance))) && 
           (!findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (!findVectorInArray(pts, new PVector(tx + grid_distance, ty + grid_distance))) && 
           (!findVectorInArray(pts, new PVector(tx, ty + grid_distance))) 
           ) {
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) {
                    line(tx, ty, tx, ty - grid_distance); // up
                    line(tx, ty, tx - grid_distance, ty); // left
                }
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx, ty - grid_distance));
                tos.add(new PVector(tx - grid_distance, ty));
                edges.put(tpv, tos);
            } 
        }
        if (
            // verticals check
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance))) && 
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty)) ^
            findVectorInArray(pts, new PVector(tx - grid_distance, ty)))
           ) {
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) line(tx, ty, tx, ty + grid_distance); // down
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!bp.contains(new PVector(tx, ty + grid_distance))) bp.add(new PVector(tx, ty + grid_distance));
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx, ty + grid_distance));
                edges.put(tpv, tos);
            }
        }
        if (
            // horiontals check
           (findVectorInArray(pts, new PVector(tx + grid_distance, ty))) && 
           (findVectorInArray(pts, new PVector(tx, ty + grid_distance)) ^
            findVectorInArray(pts, new PVector(tx, ty - grid_distance)))
           ) {
            if (tx <= width - gap && tx >= 0 && ty <= height - gap && ty >= 0) {
                if (debug) line(tx, ty, tx + grid_distance, ty); // right
                if (!bp.contains(tpv)) bp.add(tpv);
                if (!bp.contains(new PVector(tx + grid_distance, ty))) bp.add(new PVector(tx + grid_distance, ty));
                if (!edges.containsKey(tpv)) {
                    tos = new ArrayList<>();
                } else {
                    tos = edges.get(tpv);
                }
                tos.add(new PVector(tx + grid_distance, ty));
                edges.put(tpv, tos);
            }   
        }
    }
}


// givenan array of points and preferred points, create a path towards a given target
ArrayList<PVector> pathfind(ArrayList<PVector> pts, ArrayList<PVector> pref, PVector tgt) {
    ArrayList<PVector> tpvs, npt;
    if (!pref.isEmpty()) tpvs = new ArrayList<>(pref);
    else tpvs = new ArrayList<>(pts);
    int tpvi = 0;
    for (PVector tpv : tpvs) {
        if (grid_interval > 0 && tpvi % min(grid_interval, tpvs.size()) == 0) {
            int rnd_dir_x = (int)random(0, rUpperBound);
            int rnd_dir_y = (int)random(0, rUpperBound);
            int amnt_x = 0, amnt_y = 0;
            
            if (rnd_dir_x <= rLowerBound) {
                if (rnd_dir_x % 2 == 0) amnt_x = gap;
                else amnt_x = 0;
            } else if (rnd_dir_x <= rMiddleBound) {
                amnt_x =-  gap;
            } else {
                if (tgt.x >= tpv.x) amnt_x = gap;
                else amnt_x = -gap;
            }
            
            if (rnd_dir_y <= rLowerBound) {
                if (rnd_dir_y % 2 == 0) amnt_y = gap;
                else amnt_y = 0;
            } else if (rnd_dir_y <= rMiddleBound) {
                amnt_y =-  gap;
            } else {
                if (tgt.y >= tpv.y) amnt_y = gap;
                else amnt_y = -gap;
            }

            
            PVector npv = new PVector(roundToN((int)(tpv.x + amnt_x), gap), roundToN((int)(tpv.y + amnt_y), gap));
            // if (!pts.contains(npv) && dummy.contains(npv)) {
            if (!pts.contains(npv) && !dummy.contains(npv)) {
                pts.add(npv);
            }
        }
        tpvi++;
    }
    return pts;
}


