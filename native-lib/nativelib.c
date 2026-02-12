#include <string.h>

int native_add(int a, int b) {
    return a + b;
}

void native_fill_buffer(int* buf, int len) {
    for (int i = 0; i < len; i++) {
        buf[i] = i * i;
    }
}

int native_string_length(const char* s) {
    if (!s) return -1;
    return (int)strlen(s);
}

typedef struct {
    double x;
    double y;
} Point2D;

typedef struct {
    double x;
    double y;
    double width;
    double height;
} Rect2D;

double native_point_distance(Point2D a, Point2D b) {
    double dx = a.x - b.x;
    double dy = a.y - b.y;
    return dx * dx + dy * dy;
}

double native_rect_area(Rect2D r) {
    return r.width * r.height;
}

Rect2D native_rect_offset(Rect2D r, Point2D offset) {
    Rect2D result = { r.x + offset.x, r.y + offset.y, r.width, r.height };
    return result;
}
