#ifndef RT_H
#define RT_H


#define PAD_REG *(vu16*)PAD

enum Keys {

    KEY_A       = BIT(0),
    KEY_B       = BIT(1),
    KEY_SEL     = BIT(2),
    KEY_STA     = BIT(3),
    KEY_RIGHT   = BIT(4),
    KEY_LEFT    = BIT(5),
    KEY_UP      = BIT(6),
    KEY_DOWN    = BIT(7),
    KEY_R       = BIT(8),
    KEY_L       = BIT(9),
    KEY_X       = BIT(10),
    KEY_Y       = BIT(11)

};


const u32 KEY_MASK = 0xFFF;
const u32 PAD = 0x10146000;


#endif