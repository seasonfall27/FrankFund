// JavaScript source code for Create New User Account React Component

import React, { Component } from "react";
import axios from 'axios';
import swal from 'sweetalert';
import Swal from 'sweetalert2'
import { withRouter } from "react-router-dom";

class CreateUserAccount extends Component {

    constructor(props) {
        super(props);

        this.register = this.register.bind(this);
    }

    //state = {
    //    AccountUsername: '',
    //    EmailAddress: '',
    //    Password: ''
    //}

    // ----- Testing with static JSON input -----
    //onSubmit(e) {
    //    e.preventDefault();

    //    axios.post('/api/account/create&apikey=bd0eecf7cf275751a421a6101272f559b0391fa0', {
    //        AccountUsername: 'testing',
    //        EmailAddress: 'testing@gmail.com',
    //        Password: 'Password1'
    //    })
    //        .then((response) => {
    //            console.log(response);
    //        }, (error) => {
    //            console.log(error);
    //        });
    //}

    // Function that occurs when the submit button is pressed in the user account creation form
    register(e) {
        e.preventDefault();
        // Gets data from the form
        const form = document.querySelector("form");
        let data = new FormData(form);

        // Converts form data into JSON
        var object = {};
        data.forEach(function (value, key) {
            object[key] = value;
        });
        var json = JSON.stringify(object);
        console.log(json);

        let loading = true;
        while (loading) {
            Swal.fire({
                title: 'Creating new account...',
                allowOutsideClick: false,
                onBeforeOpen: () => { Swal.showLoading() },
                onAfterClose() {
                    Swal.hideLoading()
                }
            });
            // Calls axios function to post the JSON data for POST request at API endpoint
            axios({
                method: "post",
                url: "/api/account/create&apikey=bd0eecf7cf275751a421a6101272f559b0391fa0",
                data: json,
                headers: {
                    'accept': 'application/json',
                    'content-type': 'application/json'
                }
            })
                .then((res) => {
                    console.log(res);
                    Swal.close();
                    swal("Success!", "Account has successfully been created!", "success");
                    setTimeout(() => {
                        this.props.history.push("/login");
                    }, 500);
                })
                .catch((err) => {
                    //Swal.fire({
                    //    title: "Error!",
                    //    icon: "error",
                    //    html: `<p>Unable to create new account, ${err} has occured.</p>`,
                    //    showCloseButton: true
                    //})
                    Swal.close();
                    swal("Error!", "An error has occured.\nUsername or email already exists " +
                        "or the password does not meet the following complexity requirements: " +
                        "\nOne uppercase letter" +
                        "\nOne lowercase letter" +
                        "\nOne number digit", "error");
                    throw err;
                })
            // Exit loading loop
            loading = false;
        }
    }

    componentDidMount() {
        const backgroundTag = document.getElementsByClassName("inner")[0];
        backgroundTag.style.width = "30%";
    }

    render() {
        return (
            <form action="" method="post" onSubmit={this.register}>
                <h3>Create New User Account</h3>

                <div className="form-group">
                    <label>Username</label>
                    <input type="text" name="AccountUsername" className="form-control" placeholder="Username" required/>
                </div>

                <div className="form-group">
                    <label>Email</label>
                    <input type="email" name="EmailAddress" className="form-control" placeholder="Enter email" required/>
                </div>

                <div className="form-group">
                    <label>Password</label>
                    <input type="password" name="Password" className="form-control" placeholder="Enter password" required/>
                </div>

                <button type="submit" className="btn btn-dark btn-lg btn-block">Register</button>
                <p className="forgot-password text-right">
                    Already registered <a href="/login">log in?</a>
                </p>
            </form>
        );
    }
}

export default withRouter(CreateUserAccount);