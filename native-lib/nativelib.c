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
