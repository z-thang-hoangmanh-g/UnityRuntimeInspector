using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CustomStringFieldAttribute : Attribute
{
	private Action<string> onValueChange;
	public void Execute(Action<string> onValueChange)
	{
		this.onValueChange = onValueChange;
		Execute();
	}

	protected virtual void Execute()
	{
	}

	public void Set(string value)
	{
		onValueChange.Invoke(value);
	}
}
