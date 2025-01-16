import bpy

# Função para criar uma parede
def create_wall(location, width, height, depth):
    bpy.ops.mesh.primitive_cube_add(size=2, location=location)
    wall = bpy.context.object
    wall.scale[0] = width / 2
    wall.scale[1] = depth / 2
    wall.scale[2] = height / 2
    return wall

# Função para criar um rodapé
def create_baseboard(location, width, height, depth):
    bpy.ops.mesh.primitive_cube_add(size=2, location=location)
    baseboard = bpy.context.object
    baseboard.scale[0] = width / 2
    baseboard.scale[1] = depth / 2
    baseboard.scale[2] = height / 2
    return baseboard

# Função para criar um material
def create_material(name, color):
    mat = bpy.data.materials.new(name=name)
    mat.diffuse_color = color
    return mat

# Função para criar uma textura
def create_texture(image_path, texture_type):
    tex = bpy.data.textures.new(name=texture_type, type='IMAGE')
    img = bpy.data.images.load(image_path)
    tex.image = img
    return tex

# Função para aplicar múltiplas texturas a um material
def apply_textures(material, textures):
    for texture_type, texture in textures.items():
        tex_slot = material.texture_slots.add()
        tex_slot.texture = texture
        tex_slot.texture_coords = 'UV'
        if texture_type == 'albedo':
            tex_slot.use_map_color_diffuse = True
        elif texture_type == 'normal':
            tex_slot.use_map_normal = True
        elif texture_type == 'metallic':
            tex_slot.use_map_metallic = True
        elif texture_type == 'roughness':
            tex_slot.use_map_roughness = True
        elif texture_type == 'ao':
            tex_slot.use_map_ambient = True
        elif texture_type == 'height':
            tex_slot.use_map_displacement = True

# Limpa a cena atual
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

# Dimensões da parede
wall_width = 4
wall_height = 3
wall_depth = 0.1

# Localização da parede
wall_location = (0, 0, wall_height / 2)

# Cria a parede
wall = create_wall(wall_location, wall_width, wall_height, wall_depth)

# Dimensões do rodapé
baseboard_width = wall_width
baseboard_height = 0.2
baseboard_depth = 0.1

# Localização do rodapé
baseboard_location = (0, 0, baseboard_height / 2)

# Cria o rodapé
baseboard = create_baseboard(baseboard_location, baseboard_width, baseboard_height, baseboard_depth)

# Cria materiais
wall_material = create_material("WallMaterial", (0.8, 0.8, 0.8, 1))
baseboard_material = create_material("BaseboardMaterial", (0.2, 0.2, 0.2, 1))

# Cria texturas para a parede
wall_textures = {
    'albedo': create_texture("C:/Users/mbmui/Downloads/sprayed-wall-texture1-bl/albedo.png", 'albedo'),
    'normal': create_texture("C:/Users/mbmui/Downloads/sprayed-wall-texture1-bl/normal-ogl.png", 'normal'),
    'metallic': create_texture("C:/Users/mbmui/Downloads/sprayed-wall-texture1-bl/metallic.png", 'metallic'),
    'roughness': create_texture("C:/Users/mbmui/Downloads/sprayed-wall-texture1-bl/roughness.png", 'roughness'),
    'ao': create_texture("C:/Users/mbmui/Downloads/sprayed-wall-texture1-bl/ao.png", 'ao'),
    'height': create_texture("C:/Users/mbmui/Downloads/sprayed-wall-texture1-bl/height.png", 'height')
}

# Cria textura para o rodapé
baseboard_texture = create_texture("/path/to/dark_wood_texture.jpg", 'albedo')

# Aplica texturas aos materiais
apply_textures(wall_material, wall_textures)
apply_texture(baseboard_material, baseboard_texture)

# Aplica materiais
wall.data.materials.append(wall_material)
baseboard.data.materials.append(baseboard_material)
