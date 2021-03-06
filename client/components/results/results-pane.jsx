var React = require('react');
var {connect} = require('react-redux');
var SummaryTable = require('./summary-table');

function getState(state){
    return {specs: state.get('specs'), lifecycle: state.get('lifecycle-filter'), status: state.get('status-filter')};
}

function addDispatch(dispatch){
    return {dispatch: dispatch};
}

function ResultsPane(props){
    var specs = props.specs.toList().toArray().filter(x => x.hasResults());
    
    if (specs.length == 0){
        return (
            <div>
                <br />
                <br />
                <h1 style={{textAlign: 'center'}}>No results yet.</h1>
            </div>
        );
    }
    
    return (
        <div>
            <h2>Current Results</h2>
            <hr />
            
            <SummaryTable specs={specs} status={props.status} lifecycle={props.lifecycle} dispatch={props.dispatch}/>
        </div>
        
    )
}

module.exports = connect(getState, addDispatch)(ResultsPane);