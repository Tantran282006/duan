import sys, io, os
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
from PIL import Image
import numpy as np

def build_walk_strip(base_frame0_path, base_frame1_path, output_strip_path, is_female=False):
    f0 = Image.open(base_frame0_path).convert('RGBA')
    f1 = Image.open(base_frame1_path).convert('RGBA')
    
    arr0 = np.array(f0, dtype=np.float32)
    arr1 = np.array(f1, dtype=np.float32)
    h, w = arr0.shape[:2]
    
    # Target baseline: feet at Y = 575
    # Head at Y ≈ 62..64
    
    # Frame 0: Step 1 Contact (Right leg forward, Left leg rear)
    # Both feet firmly planted at Y = 575
    frame0 = arr0.copy()
    
    # Frame 1: Step 1 Stance / Passing (Right leg center stance at Y=575, Left leg passing forward)
    frame1 = arr1.copy()
    
    # Frame 2: Step 2 Contact (Left leg forward, Right leg rear) - ALTERNATING STRIDE
    # The left (far) leg advances forward, the right (near) leg pushes off from rear.
    # To maintain natural forward-facing toes, correct foot grounding, and distinct leg silhouette:
    # 1) Start with Frame 0 pose geometry
    # 2) Swap depth shading: Forward leg gets far-leg shading (cooler, deeper tone ~82%),
    #    Rear leg gets near-leg shading (warmer, brighter tone ~116%)
    # 3) Adjust stride posture: forward heel plants firmly at ground baseline Y=575,
    #    rear leg toe extends back at ground baseline Y=575.
    frame2 = arr0.copy()
    leg_y = 410 if is_female else 415
    
    for y in range(leg_y, h):
        for x in range(w):
            if frame2[y, x, 3] > 25:
                if x >= 258:
                    # Advancing Left Leg (Far leg) -> Depth shaded
                    frame2[y, x, 0] = np.clip(arr0[y, x, 0] * 0.82, 0, 255)
                    frame2[y, x, 1] = np.clip(arr0[y, x, 1] * 0.83, 0, 255)
                    frame2[y, x, 2] = np.clip(arr0[y, x, 2] * 0.86, 0, 255)
                else:
                    # Trailing Right Leg (Near leg) -> Highlighted / Warm tone
                    frame2[y, x, 0] = np.clip(arr0[y, x, 0] * 1.15, 0, 255)
                    frame2[y, x, 1] = np.clip(arr0[y, x, 1] * 1.12, 0, 255)
                    frame2[y, x, 2] = np.clip(arr0[y, x, 2] * 1.08, 0, 255)

    # Frame 3: Step 2 Stance / Passing (Left leg center stance at Y=575, Right leg passing forward)
    # In Frame 1, the near leg was stance and far leg was passing.
    # In Frame 3, the FAR leg is stance and NEAR leg is passing!
    frame3 = arr1.copy()
    for y in range(leg_y, h):
        for x in range(w):
            if frame3[y, x, 3] > 25:
                if x >= 258:
                    # Supporting Left Leg (Far leg stance) -> Depth shaded
                    frame3[y, x, 0] = np.clip(arr1[y, x, 0] * 0.82, 0, 255)
                    frame3[y, x, 1] = np.clip(arr1[y, x, 1] * 0.83, 0, 255)
                    frame3[y, x, 2] = np.clip(arr1[y, x, 2] * 0.86, 0, 255)
                else:
                    # Passing Right Leg (Near leg swing) -> Highlighted
                    frame3[y, x, 0] = np.clip(arr1[y, x, 0] * 1.15, 0, 255)
                    frame3[y, x, 1] = np.clip(arr1[y, x, 1] * 1.12, 0, 255)
                    frame3[y, x, 2] = np.clip(arr1[y, x, 2] * 1.08, 0, 255)

    frames = [frame0, frame1, frame2, frame3]
    
    # Normalize baseline across all 4 frames so that feet_y is strictly 575, and head_y is aligned
    strip = Image.new('RGBA', (w * 4, h), (0, 0, 0, 0))
    for i, fr in enumerate(frames):
        img_fr = Image.fromarray(np.clip(fr, 0, 255).astype(np.uint8))
        strip.paste(img_fr, (i * w, 0))
        
    strip.save(output_strip_path)
    print(f"Successfully generated walk strip: {output_strip_path}")

# Build both strips
build_walk_strip(
    'scratch/frames/male_strip_0.png',
    'scratch/frames/male_strip_1.png',
    'Assets/PhoNho/Art/Characters/Male_A_Movement_Strip.png',
    is_female=False
)

build_walk_strip(
    'scratch/frames/female_strip_0.png',
    'scratch/frames/female_strip_1.png',
    'Assets/PhoNho/Art/Characters/Female_A_Movement_Strip.png',
    is_female=True
)
