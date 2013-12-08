var obj = { name:"Nicolas", get Name(){ return "My name is "+this.name}, set Name(value) { this.name=value; } };

obj.name="Sébastien";
obj.Name="Nicolas";
assert("Nicolas", obj.name);
