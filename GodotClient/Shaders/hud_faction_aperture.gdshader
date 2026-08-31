shader_type canvas_item;
render_mode unshaded;

uniform sampler2D aperture_mask : filter_linear, repeat_disable;
uniform vec2 mask_source_size = vec2(256.0);
uniform float mask_source_corner = 48.0;
uniform vec4 mask_screen_rect = vec4(0.0, 0.0, 1.0, 1.0);
uniform vec2 mask_destination_size = vec2(1.0);
uniform float mask_destination_corner = 44.0;

float source_axis(float destination_pixel, float destination_size,
        float source_size, float destination_corner, float source_corner) {
    if (destination_pixel <= destination_corner) {
        return destination_pixel * source_corner / max(destination_corner, 0.001);
    }
    if (destination_pixel >= destination_size - destination_corner) {
        return source_size - (destination_size - destination_pixel) * source_corner /
            max(destination_corner, 0.001);
    }
    return source_corner + (destination_pixel - destination_corner) *
        (source_size - source_corner * 2.0) /
        max(destination_size - destination_corner * 2.0, 0.001);
}

void fragment() {
    // Fragment COLOR already contains the primitive color multiplied by its
    // default texture. Sampling TEXTURE again corrupts GUI colors and glyphs.
    vec4 source = COLOR;
    vec2 local_uv = (SCREEN_UV - mask_screen_rect.xy) /
        max(mask_screen_rect.zw, vec2(0.000001));
    if (any(lessThan(local_uv, vec2(0.0))) || any(greaterThan(local_uv, vec2(1.0)))) {
        discard;
    }

    vec2 destination_pixel = local_uv * mask_destination_size;
    vec2 source_pixel = vec2(
        source_axis(destination_pixel.x, mask_destination_size.x, mask_source_size.x,
            mask_destination_corner, mask_source_corner),
        source_axis(destination_pixel.y, mask_destination_size.y, mask_source_size.y,
            mask_destination_corner, mask_source_corner));
    float aperture = texture(aperture_mask, source_pixel / mask_source_size).a;
    source.a *= aperture;
    if (source.a <= 0.001) {
        discard;
    }
    COLOR = source;
}
