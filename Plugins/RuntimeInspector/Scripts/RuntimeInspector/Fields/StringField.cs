using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
	public class StringField : InspectorField
	{
		public enum Mode { OnValueChange = 0, OnSubmit = 1 };

#pragma warning disable 0649
		[SerializeField]
		private BoundInputField input;
		[SerializeField] private Button customFunctionBtn;
#pragma warning restore 0649

		private Mode m_setterMode = Mode.OnValueChange;
		public Mode SetterMode
		{
			get { return m_setterMode; }
			set
			{
				m_setterMode = value;
				input.CacheTextOnValueChange = m_setterMode == Mode.OnValueChange;
			}
		}

		private int lineCount = 1;
		protected override float HeightMultiplier { get { return lineCount; } }

		public override void Initialize()
		{
			base.Initialize();

			input.Initialize();
			input.OnValueChanged += OnValueChanged;
			input.OnValueSubmitted += OnValueSubmitted;
			input.DefaultEmptyValue = string.Empty;
		}

		public override bool SupportsType( Type type )
		{
			return type == typeof( string );
		}

		protected override void OnBound(MemberInfo variable,
										IEnumerable<Attribute> arrayCustomAttribute)
		{
			base.OnBound( variable , arrayCustomAttribute);

			int prevLineCount = lineCount;
			if( variable == null )
				lineCount = 1;
			else
			{
				MultilineAttribute multilineAttribute = variable.GetAttribute<MultilineAttribute>();
				if( multilineAttribute != null )
					lineCount = Mathf.Max( 1, multilineAttribute.lines );
				else if( variable.HasAttribute<TextAreaAttribute>() )
					lineCount = 3;
				else
					lineCount = 1;
			}

			if( prevLineCount != lineCount )
			{
				input.BackingField.lineType = lineCount > 1 ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
				input.BackingField.textComponent.alignment = lineCount > 1 ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;

				OnSkinChanged();
			}


			var customFunctionAttribute = (CustomStringFieldAttribute)(arrayCustomAttribute?.FirstOrDefault(x => x is CustomStringFieldAttribute))
				?? (CustomStringFieldAttribute)variable?.GetCustomAttributes().FirstOrDefault(x => x is CustomStringFieldAttribute);
			customFunctionBtn.onClick.RemoveAllListeners();
			if (customFunctionAttribute != default)
			{
				customFunctionBtn.gameObject.SetActive(true);
				customFunctionBtn.onClick.AddListener(() => customFunctionAttribute.Execute(value => Value = value));
			}
			else
			{
				customFunctionBtn.gameObject.SetActive(false);
			}

		}

		protected override void OnUnbound()
		{
			base.OnUnbound();
			SetterMode = Mode.OnValueChange;
		}

		private bool OnValueChanged( BoundInputField source, string input )
		{
			if( m_setterMode == Mode.OnValueChange )
				Value = input;

			return true;
		}

		private bool OnValueSubmitted( BoundInputField source, string input )
		{
			if( m_setterMode == Mode.OnSubmit )
				Value = input;

			Inspector.RefreshDelayed();
			return true;
		}

		protected override void OnSkinChanged()
		{
			base.OnSkinChanged();
			input.Skin = Skin;

			Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
			variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
			( (RectTransform) input.transform ).anchorMin = rightSideAnchorMin;
		}

		public override void Refresh()
		{
			base.Refresh();

			if( Value == null )
				input.Text = string.Empty;
			else
				input.Text = (string) Value;
		}
	}
}
