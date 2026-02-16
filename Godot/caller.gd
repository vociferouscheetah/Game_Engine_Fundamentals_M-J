extends Node

@export var receiver: Node

func _ready():
	print("Hello Friend")

	if receiver != null:
		receiver.OnCalled()
	else:
		push_error("Receiver reference not assigned!")
