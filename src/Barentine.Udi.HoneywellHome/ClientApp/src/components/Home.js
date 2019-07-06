import React, { Component } from 'react';
import {LocalStorageComponent} from "../SessionStorageComponent";

export class Home extends LocalStorageComponent {
  static displayName = Home.name;

  constructor(props) {
      super(props);
      this.state = {clientId: '', clientSecret: '', apiBaseUrl: ''};

      this.handleChange = this.handleChange.bind(this);
      this.authorize = this.authorize.bind(this);
  }

  componentDidMount() {
      super.componentDidMount();
      
      fetch('api/metadata/config', {
          method: 'get',
          headers: {
              'Accept': 'application/json'
          }
      }).then( response => response.json())
          .then( data => {
              this.setState({ apiBaseUrl: data.apiBaseUri });
          })
          .catch( error => {
              console.log(error);
          });
  }

  handleChange(event) {
      this.setState({
          [event.target.name]: event.target.value
      });
  }

  authorize(event) {
      event.preventDefault();
      window.location = this.state.apiBaseUrl + "/oauth2/authorize?response_type=code&client_id="
          + this.state.clientId + "&redirect_uri=" + document.location.origin + "/auth";
  }
  
  render () {
    return (
      <div>
          <form className="pure-form pure-form-aligned" name="authorization-form" onSubmit={this.authorize}>
              <fieldset>
                  <legend>Honeywell Home API Authorization</legend>
                  <div className="pure-control-group">
                      <label id='clientIdLabel' htmlFor='clientIdField'>Client ID</label>
                      <input id='clientIdField' name='clientId' type='text' required='true' value={this.state.clientId} onChange={this.handleChange}/>
                      <span className="pure-form-message-inline">This is a required field.</span>
                  </div>

                  <div className="pure-control-group">
                      <label id='clientSecretLabel' htmlFor='clientSecretField'>Client Secret</label>
                      <input id='clientSecretField' name='clientSecret' type='password' required='true' value={this.state.clientSecret} onChange={this.handleChange}/>
                      <span className="pure-form-message-inline">This is a required field.</span>
                  </div>

                  <div className="pure-controls">
                      <button type="submit" className="pure-button pure-button-primary">Log in
                      </button>
                  </div>
              </fieldset>
          </form>
      </div>
    );
  }
}
