using System;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class ElementProcessor
    {
        public event EventHandler<ParseResult> StoreEvent;

        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IElementKey _prefixer;
        private readonly IElementTerminator _terminator;

        private ParseResult? _parseResult;
        private int _maxPrefix = -1;
        private int _maxTerminator = -1;
        private ElementType _elemType = ElementType.PlainText;

        public ElementProcessor(
            IElementKey prefixer,
            IElementTerminator terminator
        )
        {
            _prefixer = prefixer;
            _terminator = terminator;
        }

        public string Text => _sb.ToString();

        public ElementType ElementType
        {
            get
            {
                _elemType = MaxPrefixLength > 0 ? ElementType.Key : ElementType.PlainText;

                return _elemType;
            }
        }

        public int MaxPrefixLength
        {
            get
            {
                if( _maxPrefix < 0 ) 
                    _maxPrefix = _prefixer.GetMaxPrefixLength( this );

                return _maxPrefix;
            }
        }

        public int MaxTerminatorLength
        {
            get
            {
                if( _maxTerminator < 0 )
                    _maxTerminator = _terminator.GetMaxTerminatorLength( this );

                return _maxTerminator;
            }
        }

        public void AddCharacter( char toAdd )
        {
            _sb.Append(toAdd);

            ForcePrefixTerminatorElementTypeUpdate();
        }

        public void Clear()
        {
            _sb.Clear();
            _parseResult = null;

            ForcePrefixTerminatorElementTypeUpdate();
        }

        public void ProcessPendingText( bool lastOption = false )
        {
            // this shouldn't happen but we can't process something
            // that doesn't have a terminator
            if( MaxTerminatorLength == 0 && !lastOption )
                return;

            // remove the prefix if we're a key
            var text = _sb.ToString();

            if( ElementType == ElementType.Key )
                text = text.Substring(MaxPrefixLength);

            // remove terminator
            text = text.Substring(0, text.Length - MaxTerminatorLength);

            switch( ElementType )
            {
                case ElementType.Key:
                    // if _parseResult is defined and we're currently a Key then
                    // the _parseResult refers to a switch (i.e., a parameterless option),
                    // so store it (StoreResultIfDefined checks to ensure _parseResult is defined)
                    StoreResultIfDefined();

                    // create a new ParseResult corresponding to the new key value
                    _parseResult = new ParseResult() { Key = text };

                    break;

                case ElementType.PlainText:
                    // if we're a plain text then we're a parameter to be added
                    // to the current ParseResult...which might not be defined
                    // if we're a naked (i.e., unkeyed) plain text value
                    _parseResult?.Parameters.Add( text );
                    break;
            }

            // take care of any pending ParseResult on the last go around
            if( lastOption )
                StoreResultIfDefined();

            // reset text accumulator
            _sb.Clear();

            ForcePrefixTerminatorElementTypeUpdate();
        }

        private void StoreResultIfDefined()
        {
            if( _parseResult == null )
                return;

            // add in a "true" value so framework conversions have something
            // to work with if this is a parameterless option (i.e., a switch)
            if (_parseResult.NumParameters == 0)
                _parseResult.Parameters.Add("true");

            StoreEvent?.Invoke(this, _parseResult!);
        }

        private void ForcePrefixTerminatorElementTypeUpdate()
        {
            // force update on next get
            _maxPrefix = -1;
            _maxTerminator = -1;
        }
    }
}